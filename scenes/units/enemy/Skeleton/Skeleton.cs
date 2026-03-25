using System;
using Game.Component;
using Game.Units;
using Godot;

namespace Game.Units;

public partial class Skeleton : Unit
{
	private UnitDetectionComponent detectionComponent;
	

	public override void _Ready()
	{
		base._Ready();

		detectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		detectionComponent.UnitDetected += OnUnitDetected;
	}

    private void OnUnitDetected(Unit target)
    {
		GD.Print("unit detected");
        attackTarget = target;
		MoveTo(target.GlobalPosition);
    }

}
