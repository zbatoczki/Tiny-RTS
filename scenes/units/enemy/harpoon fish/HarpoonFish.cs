using Godot;
using Game.Units;
using Game.Component;
using System.Runtime.CompilerServices;

public partial class HarpoonFish : Unit
{
	[Export] private PackedScene projectileScene;
	private UnitDetectionComponent detectionComponent;
	
	public override void _Ready()
	{
		base._Ready();

		detectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		detectionComponent.UnitDetected += OnUnitDetected;
		detectionComponent.Scale *= stats.VisionRange;
	}

	public override void Attack()
	{
		if (!IsInstanceValid(AttackTarget)) return;
		
		var projectile = projectileScene.Instantiate<ProjectileComponent>();
		Owner.CallDeferred(Node.MethodName.AddChild, projectile);
		projectile.Launch(GlobalPosition, stats.AttackDamage, AttackTarget);
	}

    private void OnUnitDetected(Unit target)
    {
		GD.Print("unit detected");
        AttackTarget = target;
		MoveTo(target.GlobalPosition);
    }
}
