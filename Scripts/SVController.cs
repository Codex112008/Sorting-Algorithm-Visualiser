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
	[Export] private Slider delaySlider;
	[Export] private RichTextLabel delayDisplay;

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

		delayDisplay.Text = delaySlider.Value + "ms";
	}

	public async void SortArray()
	{
		if (sorting == true)
		{
			sorting = false;
			await Wait(-1);
		}

		switch ((SortingMethod)sortOptionsDropdown.Selected)
		{
			case SortingMethod.CombSort: 
				sorting = true;
				await CombSort(); 
			break;
			case SortingMethod.BubbleSort: 
				sorting = true;
				await BubbleSort(); 
			break;
			case SortingMethod.StalinSort: 
				sorting = true;
				await StalinSort(); 
			break;
			case SortingMethod.BogoSort: 
				sorting = true;
				await BogoSort(); 
			break;
			case SortingMethod.QuickSort:
				sorting = true;
				await QuickSort(0, GetChildCount() - 1);
			break;
			case SortingMethod.SelectionSort:
				sorting = true;
				await SelectionSort();
			break;
		}
	}

	public void UpdateArray()
	{
		sorting = false;
		shuffling = false;

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

	public async Task Wait(int i)
	{
		int divisor = arrLengthSlider.Value > 750 ? 8 :
              	arrLengthSlider.Value > 500 ? 4 :
              	arrLengthSlider.Value > 250 ? 2 : 1;
		if (i % divisor == 0 || i == -1 || delaySlider.Value > 50)
			await ToSignal(GetTree().CreateTimer(delaySlider.Value / 1000), "timeout");
	}

	public static float GetHeight(Node node)
	{
        if (node is ColorRect rect)
            return rect.CustomMinimumSize.Y;
		else
			return -1;
    }

	public async void ShuffleChildren()
	{
		if (sorting == true || shuffling == true)
			return;

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

			await Wait(i);

			Node temp = GetChild(randInt);
			MoveChild(children[i], randInt);
			MoveChild(temp, i);
		}
		selectedRects.Clear();

		shuffling = false;
	}

	public async Task QuickSort(int low, int high)
	{
		if (low < high && low >= 0 && high >= 0)
		{
			Node pivot = GetChild(low);

			int i = low - 1;
			int j = high + 1;
			while (true)
			{
				sorting = true;

				do 
					i++;
				while(GetHeight(GetChildren()[i]) < GetHeight(pivot));

				do 
					j--;
				while(GetHeight(GetChildren()[j]) > GetHeight(pivot));

				if (i >= j)
					break;

				selectedRects.Add(GetChild<ColorRect>(i));
				selectedRects.Add(GetChild<ColorRect>(j));

				await Wait(i);

				Node temp = GetChild(j);
				MoveChild(GetChild(i), j);
				MoveChild(temp, i);

				selectedRects.Remove(GetChild<ColorRect>(i));
				selectedRects.Remove(GetChild<ColorRect>(j));
			}

            _ = QuickSort(low, j);
            _ = QuickSort(j + 1, high);
		}
		else
			sorting = false;
	}

	public async Task CombSort()
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
    	    	if (GetHeight(GetChild(i)) > GetHeight(GetChild(i + gap)))
    	    	{
					selectedRects.Clear();

					if (!sorting)
						return;

					selectedRects.Add(GetChild<ColorRect>(i + gap));
					selectedRects.Add(GetChild<ColorRect>(i));

					await Wait(i);

         			// Swap the two values
        	  		MoveChild(children[i], i + gap);
        	  		MoveChild(children[i + gap], i);
          
        	  		// If swapped, check next pass if sorted
        	  		sorted = false;
        		}
      		}
    	}
		selectedRects.Clear();

		sorting = false;
  	}

	public async Task SelectionSort()
	{
		for (int i = 0; i < GetChildCount() - 1; i++)
		{
			Array<Node> children = GetChildren();

			selectedRects.Clear();

			int min = i;
			for (int j = i + 1; j < GetChildCount(); j++)
			{
				selectedRects.Clear();
				if (GetHeight(GetChild(j)) < GetHeight(GetChild(min)))
				{
					min = j;
					selectedRects.Add(GetChild<ColorRect>(min));
				}
			}
				
			selectedRects.Add(GetChild<ColorRect>(min));
			selectedRects.Add(GetChild<ColorRect>(i));

			await Wait(i);
			
			MoveChild(children[min], i);
			MoveChild(children[i], min);
		}
		selectedRects.Clear();

		sorting = false;
	}

	public async Task BubbleSort()
	{
		for (int j = 1; j < GetChildCount() - 1; j++)
		{
			for (int i = 0; i < GetChildCount() - j; i++)
			{
				Array<Node> children = GetChildren();
				if (GetHeight(GetChild(i)) > GetHeight(GetChild(i + 1)))
				{
					selectedRects.Clear();

					if (!sorting)
						return;

					selectedRects.Add(GetChild<ColorRect>(i + 1));
					selectedRects.Add(GetChild<ColorRect>(i));

					await Wait(i);

					// Swap the two values
					MoveChild(children[i], i + 1);
					MoveChild(children[i + 1], i);
				}
			}
		}
		selectedRects.Clear();

		sorting = false;
	}

	public async Task StalinSort() 
	{
    	for (int i = 0; i < GetChildCount() - 1; i++) 
		{
      		if (GetHeight(GetChild(i)) > GetHeight(GetChild(i + 1))) 
			{
				selectedRects.Clear();

				if (!sorting)
					return;

				selectedRects.Add(GetChild<ColorRect>(i + 1));

				await Wait(i);

        		GetChild(i + 1).QueueFree();
        		i--;
      		}
    	}

		sorting = false;
  	}

	public async Task BogoSort()
	{
		bool sorted = false;
    	while (!sorted)
		{
			sorted = true;
			for (int i = 0; i < GetChildCount() - 1; i++) 
			{
      			if (GetHeight(GetChild(i)) > GetHeight(GetChild(i + 1)))
        			sorted = false;
    		}

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

				await Wait(i);

				Node temp = GetChild(randInt);
				MoveChild(children[i], randInt);
				MoveChild(temp, i);
			}
			selectedRects.Clear();
		}

		sorting = false;
  	}
}

enum SortingMethod
{
	CombSort = 0,
	BubbleSort = 1,
	StalinSort = 2,
	BogoSort = 3,
	QuickSort = 4,
	SelectionSort = 5
}