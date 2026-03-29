using Godot;
using Game.Component;

namespace Game.Units;

public partial class RangedUnit : Unit
{
	[Export] private PackedScene projectileScene;
	[Export] private int ProjectileLaunchAnimationFrame;
	private UnitDetectionComponent detectionComponent;
	
	public override void _Ready()
	{
		base._Ready();

		detectionComponent = GetNode<UnitDetectionComponent>(nameof(UnitDetectionComponent));
		detectionComponent.UnitDetected += OnUnitDetected;
		detectionComponent.Scale *= stats.VisionRange;

		animatedSprite2D.FrameChanged += OnFrameChanged;
	}

	public override void Attack()
	{
		if (!IsInstanceValid(AttackTarget) || animatedSprite2D.Frame != ProjectileLaunchAnimationFrame) return;
		
		var projectile = projectileScene.Instantiate<ProjectileComponent>();
		Owner.CallDeferred(Node.MethodName.AddChild, projectile);
		projectile.Launch(GlobalPosition, stats.AttackDamage, AttackTarget);
	}


	private void OnFrameChanged()
	{
		if(animatedSprite2D.Animation == "attack" && animatedSprite2D.Frame == ProjectileLaunchAnimationFrame)
			Attack();
	}
}
