using Game.Autoload;
using Game.Component;
using Game.Globals;
using Game.Units;
using Godot;

namespace Game.Buildings;

public partial class Castle : Building
{
	[Export] public PackedScene WorkerScene;

	private UnitDetectionComponent unitDetectionComponent;

	public override void _Ready()
	{
		unitDetectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		unitDetectionComponent.UnitDetected += OnUnitDetected;
	}

	protected override bool TrainUnit()
	{
		var (goldCost, woodCost) = Costs.Worker;
		if(!GameManager.Instance.CanTrain(Faction.Player)) return false;
		if(!GameManager.Instance.CanAffordBuilding(woodCost, goldCost)) return false;
		Enqueue(WorkerScene, TrainTime);
		return true;
	}

	private void OnUnitDetected(Unit unit)
	{
		if(unit is Worker worker && worker.HasInventory)
		{
			worker.DropOffResources();
		}
	}
}
