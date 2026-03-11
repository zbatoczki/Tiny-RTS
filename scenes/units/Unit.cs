using Game.Autoload;
using Game.InputMap;
using Godot;

namespace Game.Units;

public partial class Unit : CharacterBody2D
{
	private readonly StringName PLAYER_UNIT_GROUP = "PlayerUnit";

    [Export] public float MoveSpeed = 150f;

    [Export] public bool UsePathfinding = true;

	[Export]
	private Color borderColor;
	private HealthComponent healthComponent;

	public bool IsSelected { get; private set; } = false;
    public bool IsMoving    { get; private set; } = false;

    private Vector2 directTarget    = Vector2.Zero;
    private bool    hasDirectTarget  = false;

    private CollisionShape2D  collisionShape;
    private NavigationAgent2D _navAgent;
    private AnimatedSprite2D animatedSprite2D;

    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));

        collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        _navAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        // Ensure this unit is in the "units" group so SelectionManager can
        // find it. You can also add the group in the Godot editor.
        AddToGroup(PLAYER_UNIT_GROUP);
    }

    public override void _PhysicsProcess(double _)
    {
        if (UsePathfinding)
            ProcessNavAgentMovement();
        else
            ProcessDirectMovement();
    }

    public void MoveTo(Vector2 worldTarget)
    {
        IsMoving = true;
        animatedSprite2D.Play("move");
        if (UsePathfinding)
        {
            if (_navAgent == null)
            {
                GD.PushWarning($"{Name}: Cannot move — UsePathfinding is true but NavigationAgent2D is missing.");
                return;
            }
            _navAgent.TargetPosition = worldTarget;
        }
        else
        {
            directTarget    = worldTarget;
            hasDirectTarget = true;
        }
    }

	 public virtual void SetSelected(bool selected)
    {
        IsSelected = selected;

        healthComponent.Visible = selected;

        OnSelectionChanged(selected);
    }

    public bool ContainsPoint(Vector2 worldPoint)
    {
		GD.Print(collisionShape.Shape);
        if (collisionShape?.Shape is CircleShape2D circle)
        {
			float worldRadius = circle.Radius * collisionShape.GlobalTransform.Scale.X;
            return collisionShape.GlobalPosition.DistanceTo(worldPoint) <= worldRadius;
        }

        if (collisionShape?.Shape is RectangleShape2D rect)
        {
            // Transform point into local space of the collision shape.
            Vector2 local = collisionShape.GlobalTransform.AffineInverse() * worldPoint;
            Rect2 box = new(-rect.Size / 2f, rect.Size);
            return box.HasPoint(local);
        }

        Vector2 center = collisionShape?.GlobalPosition ?? GlobalPosition;
		return center.DistanceTo(worldPoint) <= 24f;
    }

    protected virtual void OnSelectionChanged(bool selected)
    {
        GD.Print($"{Name} selected={selected}");
    }

    private void ProcessNavAgentMovement()
    {
        if (_navAgent == null || _navAgent.IsNavigationFinished())
        {
            IsMoving = false;
            animatedSprite2D.Play("idle");
            Velocity = Vector2.Zero;
            return;
        }

        Vector2 nextPoint = _navAgent.GetNextPathPosition();
        Velocity = (nextPoint - GlobalPosition).Normalized() * MoveSpeed;
        MoveAndSlide();

        // Snap to destination when close enough to avoid jittering.
        if (GlobalPosition.DistanceTo(_navAgent.TargetPosition) < 4f)
        {
            GlobalPosition           = _navAgent.TargetPosition;
            _navAgent.TargetPosition = GlobalPosition; // clears the path
            IsMoving                 = false;
            animatedSprite2D.Play("idle");
            Velocity                 = Vector2.Zero;
        }
    }

    private void ProcessDirectMovement()
    {
        GD.Print("Direct Movent to position");
        if (!hasDirectTarget)
        {
            Velocity = Vector2.Zero;
            return;
        }

        if (GlobalPosition.DistanceTo(directTarget) < 4f)
        {
            GlobalPosition   = directTarget;
            hasDirectTarget = false;
            IsMoving         = false;
            animatedSprite2D.Play("idle");
            Velocity         = Vector2.Zero;
            return;
        }

        var direction = (directTarget - GlobalPosition).Normalized();
        Velocity = direction * MoveSpeed;
        animatedSprite2D.FlipH = direction.X < 0;
        MoveAndSlide();
    }

}
