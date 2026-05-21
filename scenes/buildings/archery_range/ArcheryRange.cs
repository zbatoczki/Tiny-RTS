using Game.Autoload;
using Game.Globals;
using Godot;

namespace Game.Buildings;

public partial class ArcheryRange : Building
{
	
	[Export] public PackedScene ArcherScene;

	public override void _Ready()
	{
		OnReady();
	}

    public override bool TrainUnit(UnitTypes _)
    {
        var (goldCost, woodCost) = Costs.Archer;
		PackedScene unitScene = ArcherScene;
		if(!GameManager.Instance.CanTrain(Faction.Player)) return false;
		if(!ResourceManager.Instance.Spend(Faction, woodCost, goldCost)) return false;
		Enqueue(unitScene, TrainTime);
		return true;
    }

}
