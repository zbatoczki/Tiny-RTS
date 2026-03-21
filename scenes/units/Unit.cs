using Game.Autoload;
using Godot;

namespace Game.Units;

public partial class Unit : CharacterBody2D
{
    [Signal]
    public delegate void MovementCompletedEventHandler(Vector2I gridCellPosition);

	private readonly StringName PLAYER_UNIT_GROUP = "PlayerUnit";

    [Export] public float MoveSpeed = 150f;

	private HealthComponent healthComponent;

	public bool IsSelected { get; private set; } = false;

    private Vector2 directTarget    = Vector2.Zero;

    private CollisionShape2D  collisionShape;
    private AnimatedSprite2D animatedSprite2D;

    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));

        collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        AddToGroup(PLAYER_UNIT_GROUP);

        UnitsEvents.EmitUnitMovementFinished(GlobalPosition);
    }

    public override void _PhysicsProcess(double _)
    {
        if(directTarget == Vector2.Zero) return;

        ProcessDirectMovement();
    }

    public void MoveTo(Vector2 worldTarget)
    {
        animatedSprite2D.Play("move");
        directTarget    = worldTarget;
        UnitsEvents.EmitUnitMovementStarted(GlobalPosition);
    }

	 public virtual void SetSelected(bool selected)
    {
        IsSelected = selected;

        healthComponent.Visible = selected;

        OnSelectionChanged(selected);
    }

    protected virtual void OnSelectionChanged(bool selected)
    {
        GD.Print($"{Name} selected={selected}");
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
        MoveAndSlide();
    }

    private void FinishMoving()
    {
        GlobalPosition   = directTarget;
        directTarget = Vector2.Zero;
        Velocity         = Vector2.Zero;
        animatedSprite2D.Play("idle");
        UnitsEvents.EmitUnitMovementFinished(GlobalPosition);
    }
}
