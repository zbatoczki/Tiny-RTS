using Game.Resources.Gathering;
using Godot;

namespace Game.Resources;

public partial class Tree : ResourceNode
{
	private AnimatedSprite2D animatedSprite2D;
	private CollisionShape2D collisionShape2D;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeResource();
		animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
		collisionShape2D = GetNode<CollisionShape2D>(nameof(CollisionShape2D));
	}


	

}
