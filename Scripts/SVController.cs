using Godot;
using Godot.Collections;
using System.Linq;
using System.Threading.Tasks;

public partial class SVController : HBoxContainer
{
	[Export] private Color selectedColor;
	[Export] private Slider arrLengthSlider;
	[Export] private RichTextLabel lengthDisplay;
	[Export] private OptionButton sortOptionsDropdown;

	private double value;
	private bool shuffling;
	private bool sorting;
	private Array<ColorRect> selectedRects = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (value != arrLengthSlider.Value)
		{
			UpdateArray();
			value = arrLengthSlider.Value;
		}

		foreach (ColorRect rect in GetChildren().Cast<ColorRect>())
		{
			if (selectedRects.Contains(rect))
				rect.Color = selectedColor;
			else
				rect.Color = new(1, 1, 1, 1);
		}
	}

	public void SortArray()
	{
		switch (sortOptionsDropdown.Selected)
		{
			case 0: 
				CombSort(); 
				sorting = true;
			break;
			case 1: 
				BubbleSort(); 
				sorting = true;
			break;
			case 2: 
				StalinSort(); 
				sorting = true;
			break;
			case 3: 
				BogoSort(); 
				sorting = true;
			break;
		}
	}

	public void UpdateArray()
	{
		sorting = false;

		foreach (Node child in GetChildren())
			child.QueueFree();
		
		for (int i = 0; i < arrLengthSlider.Value; i++)
		{
            ColorRect colorRect = new()
            {
                CustomMinimumSize = new(0, Size.Y / (float) arrLengthSlider.Value * (i + 1)),
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkEnd
            };
			AddChild(colorRect);
        }

		lengthDisplay.Text = arrLengthSlider.Value.ToString();
	}

	public async Task Wait(float seconds)
	{
		await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
	}

	public bool ArraySorted() 
	{
    	for (int i = 0; i < GetChildCount() - 1; i++) 
		{
      		if (GetChild<ColorRect>(i).CustomMinimumSize.Y > GetChild<ColorRect>(i + 1).CustomMinimumSize.Y)
        	return false;
    	}
    	return true;
  	}

	public async void ShuffleChildren()
	{
		shuffling = true;

		RandomNumberGenerator rand = new();
		Array<Node> children = GetChildren();
		for (int i = 0; i < GetChildCount(); i++)
		{
			selectedRects.Clear();

			if (shuffling == false)
				return;

			int randInt = rand.RandiRange(0, GetChildCount());

			selectedRects.Add(GetChild<ColorRect>(randInt));
			selectedRects.Add(GetChild<ColorRect>(i));

			Node temp = GetChild(randInt);
			MoveChild(children[i], randInt);
			MoveChild(temp, i);
			
			int divisor = arrLengthSlider.Value > 750 ? 8 :
              	arrLengthSlider.Value > 500 ? 4 :
              	arrLengthSlider.Value > 250 ? 2 : 1;
			if (i % divisor == 0)
   				await Wait(0.001f);
		}
		selectedRects.Clear();

		shuffling = false;
	}

	public async void CombSort()
  	{
    	int gap = GetChildCount();
    	bool sorted = false;
    	while (!sorted)
    	{
      		// Shrink gap and if its under 1 make it 1;
    	  	gap = Mathf.FloorToInt(gap / 1.3f);
    	  	if (gap <= 1)
    	  	{
    	    	gap = 1;
    	    	sorted = true;
    	  	}

    	 	// Rule of 11s, apparently they are better than 10 or 9
    	  	if (gap == 9 || gap == 10)
    	    	gap = 11;

    	  	// Iterate through array
    	  	for (int i = 0; i + gap < GetChildCount(); i++)
    	  	{
				Array<Node> children = GetChildren();
    	    	// If arr[i] is greater than the arr[i + gap] then swap
    	    	if (GetChild<ColorRect>(i).CustomMinimumSize.Y > GetChild<ColorRect>(i + gap).CustomMinimumSize.Y)
    	    	{
					selectedRects.Clear();

					if (!sorting)
						return;

					selectedRects.Add(GetChild<ColorRect>(i + gap));
					selectedRects.Add(GetChild<ColorRect>(i));

         			// Swap the two values
        	  		MoveChild(children[i], i + gap);
        	  		MoveChild(children[i + gap], i);
          
        	  		// If swapped, check next pass if sorted
        	  		sorted = false;

					await Wait(0.001f);
        		}
      		}
    	}
		selectedRects.Clear();

		sorting = false;
  	}

	public async void BubbleSort()
	{
		while (!ArraySorted())
		{
			for (int i = 0; i < GetChildCount() - 1; i++)
			{
				Array<Node> children = GetChildren();
				if (GetChild<ColorRect>(i).CustomMinimumSize.Y > GetChild<ColorRect>(i + 1).CustomMinimumSize.Y)
				{
					selectedRects.Clear();

					if (!sorting)
						return;

					selectedRects.Add(GetChild<ColorRect>(i + 1));
					selectedRects.Add(GetChild<ColorRect>(i));

					// Swap the two values
					MoveChild(children[i], i + 1);
					MoveChild(children[i + 1], i);

					await Wait(0.001f);
				}
			}
		}
		selectedRects.Clear();

		sorting = false;
	}

	public async void StalinSort() 
	{
    	for (int i = 0; i < GetChildCount() - 1; i++) 
		{
      		if (GetChild<ColorRect>(i).CustomMinimumSize.Y > GetChild<ColorRect>(i + 1).CustomMinimumSize.Y) 
			{
				selectedRects.Clear();

				if (!sorting)
					return;

				selectedRects.Add(GetChild<ColorRect>(i + 1));

        		GetChild(i + 1).QueueFree();
        		i--;

				await Wait(0.001f);
      		}
    	}

		sorting = false;
  	}

	public async void BogoSort()
	{
    	while (!ArraySorted())
		{
			RandomNumberGenerator rand = new();
			Array<Node> children = GetChildren();
			for (int i = 0; i < GetChildCount(); i++)
			{
				selectedRects.Clear();

				if (!sorting)
					return;

				int randInt = rand.RandiRange(0, GetChildCount());

				selectedRects.Add(GetChild<ColorRect>(randInt));
				selectedRects.Add(GetChild<ColorRect>(i));

				Node temp = GetChild(randInt);
				MoveChild(children[i], randInt);
				MoveChild(temp, i);

				await Wait(0.001f);
			}
			selectedRects.Clear();
		}

		sorting = false;
  	}
}