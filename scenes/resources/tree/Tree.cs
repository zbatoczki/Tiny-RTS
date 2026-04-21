using Game.Resources.Gathering;
using Godot;

namespace Game.Resources;

public partial class Tree : StaticBody2D
{
	[Export] public GatheringResource GatheringResource;

	private AnimatedSprite2D animatedSprite2D;
	private Sprite2D depletedSprite;
	private CollisionShape2D collisionShape2D;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
		depletedSprite = GetNode<Sprite2D>("DepletedSprite");
		collisionShape2D = GetNode<CollisionShape2D>(nameof(CollisionShape2D));
		depletedSprite.Visible = false;
		GatheringResource.ResourceDepleted += OnResourceDepleted;
	}

    private void OnResourceDepleted(Vector2I _)
    {
		QueueFree();
        // animatedSprite2D.Stop();
		// animatedSprite2D.Visible = false;
		// collisionShape2D.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		
		// depletedSprite.Visible = true;
    }
}
