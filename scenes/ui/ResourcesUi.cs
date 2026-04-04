using Game.Component.Ui;
using Godot;

namespace Game.UI;


public partial class ResourcesUi : PanelContainer
{
	
	private NumberLabel woodCountLabel;
	private NumberLabel goldCountLabel;
	private NumberLabel foodCountLabel;

	public override void _Ready()
	{
		woodCountLabel = GetNode<NumberLabel>("%WoodCountLabel");
		goldCountLabel = GetNode<NumberLabel>("%GoldCountLabel");
		foodCountLabel = GetNode<NumberLabel>("%FoodCountLabel");

		woodCountLabel.Text = "0";
		goldCountLabel.Text = "0";
		foodCountLabel.Text = "0";
	}

	public void UpdateResouceCounts(int woodCount, int goldCount, int foodCount)
	{
		woodCountLabel.AnimateTo(woodCount);
		goldCountLabel.AnimateTo(goldCount);
		foodCountLabel.AnimateTo(foodCount);
	}
}

	
