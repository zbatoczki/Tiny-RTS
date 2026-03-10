using Game.Autoload;
using Game.InputMap;
using Godot;

namespace Game.Units;

public partial class Unit : CharacterBody2D
{
	private readonly StringName PLAYER_UNIT_GROUP = "PlayerUnit";


	[Export]
	private Color borderColor;
	private HealthComponent healthComponent;
	private Rect2 selectedRect;
	private int selectedRectWidth = 1;

	public bool IsSelected { get; private set; } = false;
    private CollisionShape2D  collisionShape;

    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));

        collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        // Ensure this unit is in the "units" group so SelectionManager can
        // find it. You can also add the group in the Godot editor.
        AddToGroup(PLAYER_UNIT_GROUP);
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

}
