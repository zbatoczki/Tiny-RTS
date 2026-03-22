using Godot;
using Game.Component;
namespace Game.Units;

public partial class Unit : CharacterBody2D
{
    [Signal]
    public delegate void MovementCompletedEventHandler(Vector2I gridCellPosition);

    [Export] public float MoveSpeed = 150f;

	private HealthComponent healthComponent;

    private Vector2 directTarget    = Vector2.Zero;
    private CollisionShape2D  collisionShape;
    private AnimatedSprite2D animatedSprite2D;
    private DamageComponent damageComponent;


    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));

        collisionShape = GetNodeOrNull<CollisionShape2D>(nameof(CollisionShape2D));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        damageComponent = GetNode<DamageComponent>(nameof(DamageComponent));

        damageComponent.AttackingTarget += OnAttackingTarget;
    }

    public override void _PhysicsProcess(double _)
    {
        if(directTarget == Vector2.Zero) return;

        ProcessDirectMovement();
    }

#region SELECTION
    public virtual void SetSelected(bool selected)
    {
        healthComponent.Visible = selected;
        OnSelectionChanged(selected);
    }

    protected virtual void OnSelectionChanged(bool selected)
    {
        GD.Print($"{Name} selected={selected}");
    }
#endregion

#region MOVEMENT
    public void MoveTo(Vector2 worldTarget)
    {
        animatedSprite2D.Play("move");
        directTarget    = worldTarget;
    }

    private void ProcessDirectMovement()
    {
        if (directTarget == Vector2.Zero)
        {
            Velocity = Vector2.Zero;
            return;
        }

        if (GlobalPosition.DistanceTo(directTarget) < 4f)
        {
            FinishMoving();
            return;
        }

        var direction = (directTarget - GlobalPosition).Normalized();
        Velocity = direction * MoveSpeed;
        animatedSprite2D.FlipH = direction.X < 0;
        damageComponent.FlipH(animatedSprite2D.FlipH);
        MoveAndSlide();
    }

    private void FinishMoving()
    {
        GlobalPosition   = directTarget;
        directTarget = Vector2.Zero;
        Velocity         = Vector2.Zero;
        animatedSprite2D.Play("idle");
    }

    private void OnAttackingTarget()
    {
        directTarget = Vector2.Zero;
        Velocity         = Vector2.Zero;
        animatedSprite2D.Play("attack");
    }
#endregion
}
