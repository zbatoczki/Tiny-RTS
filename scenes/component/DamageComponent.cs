using Godot;
using System;

namespace Game.Component;

public partial class DamageComponent : Node2D
{
	[Signal]
	public delegate void AttackingTargetEventHandler();

	[Export]
	private AnimatedSprite2D animatedSprite2D;

	[Export]
	private StringName targetGroup;

	private Area2D hitBox;
	private CollisionShape2D hitBoxArea;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitBox = GetNode<Area2D>("HitBox");
		hitBoxArea = GetNode<CollisionShape2D>("%HitBoxArea");

		hitBox.BodyEntered += OnBodyEntered;
	}

	public void FlipH(bool flip)
	{
		Scale = flip ? new Vector2(-1, 1): new Vector2(1, 1);
	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print($"body Entered: {body.GetGroups()}");
		if(body is CharacterBody2D && body.IsInGroup(targetGroup))
		{
			animatedSprite2D.Play("attack");
		}
	}
}
