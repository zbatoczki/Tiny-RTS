using Game.Autoload;
using Game.Globals;
using Godot;

namespace Game.Buildings;

public partial class Barracks : Building
{
	[Export] public PackedScene WarriorScene;
	[Export] public PackedScene SpearmanScene;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OnReady();
	}

    public override bool TrainUnit(UnitTypes unitType)
    {
        var (goldCost, woodCost) = Costs.Warrior;
		PackedScene unitScene = WarriorScene;
		if(unitType == UnitTypes.Spearman)
		{
			(goldCost, woodCost) = Costs.Spearman;
			unitScene = SpearmanScene;
		}
		if(!GameManager.Instance.CanTrain(Faction.Player)) return false;
		if(!ResourceManager.Instance.Spend(Faction, woodCost, goldCost)) return false;
		Enqueue(unitScene, TrainTime);
		return true;
    }

}
