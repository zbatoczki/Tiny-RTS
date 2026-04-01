using Game.Component;

namespace Game.Units;

public partial class MeleeUnit : Unit
{

	protected UnitDetectionComponent detectionComponent;
	
	public override void _Ready()
	{
		base._Ready();

		detectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		detectionComponent.UnitDetected += OnUnitDetected;
		detectionComponent.Scale *= stats.VisionRange;
	}
}
