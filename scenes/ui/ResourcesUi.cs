using Godot;

namespace Game.UI;


public partial class ResourcesUi : PanelContainer
{
	
	private Label woodCountLabel;
	private Label goldCountLabel;
	private Label foodCountLabel;

	public override void _Ready()
	{
		woodCountLabel = GetNode<Label>("%WoodCountLabel");
		goldCountLabel = GetNode<Label>("%GoldCountLabel");
		foodCountLabel = GetNode<Label>("%FoodCountLabel");

		woodCountLabel.Text = "0";
		goldCountLabel.Text = "0";
		foodCountLabel.Text = "0";
	}

	public void UpdateResouceCounts(int woodCount, int goldCount, int foodCount)
	{
		woodCountLabel.Text = woodCount.ToString();
		goldCountLabel.Text = goldCount.ToString();
		foodCountLabel.Text = foodCount.ToString();
	}
}

	
