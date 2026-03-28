using Godot;
using Game.Units;
using Game.Component;

public partial class HarpoonFish : Unit
{
	private UnitDetectionComponent detectionComponent;
	
	public override void _Ready()
	{
		base._Ready();

		detectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		detectionComponent.UnitDetected += OnUnitDetected;
		detectionComponent.Scale *= stats.VisionRange;
	}

    private void OnUnitDetected(Unit target)
    {
		GD.Print("unit detected");
        AttackTarget = target;
		MoveTo(target.GlobalPosition);
    }
}
