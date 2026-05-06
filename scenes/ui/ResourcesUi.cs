using Game.Component.Ui;
using Game.Globals;
using Game.Autoload;
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

		woodCountLabel.SetImmediate(ResourceManager.Instance.GetWood(Faction.Player));
		goldCountLabel.SetImmediate(ResourceManager.Instance.GetGold(Faction.Player));
		foodCountLabel.SetImmediate(ResourceManager.Instance.GetFood(Faction.Player));
		
		ResourceManager.Instance.ResourceChanged += UpdateResouceCounts;
	}

	public override void _ExitTree()
    {
        ResourceManager.Instance.ResourceChanged -= UpdateResouceCounts;
    }

	public void UpdateResouceCounts(int faction, int gold, int wood, int food)
	{
		if(faction != (int) Faction.Player) return;

		GD.Print($"{woodCountLabel} |{goldCountLabel} |{foodCountLabel}");

		woodCountLabel.AnimateTo(wood);
		goldCountLabel.AnimateTo(gold);
		foodCountLabel.AnimateTo(food);
	}
}

	
