using Godot;
using System;

namespace Game.Component;

public partial class DamageComponent : Area2D
{
	[Export]
	private StringName targetGroup;

	private Area2D hitBox;
	private CollisionShape2D hitBoxArea;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitBoxArea = GetNode<CollisionShape2D>("HitBoxArea");
	}

	public void FlipH(bool flip)
	{
		Scale = flip ? new Vector2(-1, 1): new Vector2(1, 1);
	}
}
