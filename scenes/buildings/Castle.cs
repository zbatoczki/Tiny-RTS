using Game.Component;
using Game.Resources.Building;
using Game.Units;
using Godot;

namespace Game.Buildings;

public partial class Castle : Building
{
	private UnitDetectionComponent unitDetectionComponent;

	public override void _Ready()
	{
		unitDetectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		unitDetectionComponent.UnitDetected += OnUnitDetected;
	}

	private void OnUnitDetected(Unit unit)
	{
		if(unit is Worker worker && worker.HasInventory)
		{
			worker.DropOffResources();
		}
	}
}
