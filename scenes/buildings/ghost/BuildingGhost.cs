using Game.Globals;
using Godot;
using System;

public partial class BuildingGhost : Node2D
{
	[Export] private bool runAnimations = true;
	
	private readonly StringName DEFAULT_ANIMATION = "default";
	private Node2D topLeft;
	private Node2D bottomLeft;
	private Node2D topRight;
	private Node2D bottomRight;
	private Node2D upDownRoot;
	private Node2D spriteRoot;

	private Tween spriteTween;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		topLeft = GetNode<Node2D>("TopLeft");
       	bottomLeft = GetNode<Node2D>("BottomLeft");
       	topRight = GetNode<Node2D>("TopRight");
       	bottomRight = GetNode<Node2D>("BottomRight");
       	upDownRoot = GetNode<Node2D>("%UpDownRoot");
       	spriteRoot = GetNode<Node2D>("SpriteRoot");

		if (runAnimations)
		{
			RunUpDownTween();
			
			AnimationPlayer animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
			animationPlayer.Play(DEFAULT_ANIMATION);
		}
	}

	/// <summary>Tints the ghost red to indicate an invalid placement.</summary>
	public void SetInvalid()
	{
		Modulate = Colors.Red;
		upDownRoot.Modulate = Modulate;
	}

	/// <summary>Tints the ghost white to indicate a valid placement.</summary>
	public void SetValid()
	{
		Modulate = Colors.White;
		upDownRoot.Modulate = Modulate;
	}

	/// <summary>Positions the corner markers to outline a footprint of the given size.</summary>
	public void SetMarkerDimensions(Vector2I dimensions)
	{
		bottomLeft.Position = dimensions * new Vector2I(0, GlobalValues.CELL_SIZE);
		bottomRight.Position = dimensions *  new Vector2I(GlobalValues.CELL_SIZE, GlobalValues.CELL_SIZE);
		topRight.Position = dimensions *  new Vector2I(GlobalValues.CELL_SIZE, 0);
	}

	/// <summary>Adds the building's sprite scene under the bobbing root.</summary>
	public void AddSpriteNode(Node2D spriteNode)
	{
		upDownRoot.AddChild(spriteNode);
	}

	/// <summary>Tweens the sprite toward the ghost's new cell with a springy ease.</summary>
	public void DoHoverAnimation()
	{
		if (!runAnimations)
		{
			return;
		}

		if(spriteTween != null && spriteTween.IsValid())
		{
			spriteTween.Kill();
		}
		spriteTween = CreateTween();
		spriteTween
			.TweenProperty(spriteRoot, "global_position", GlobalPosition, 0.3)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
	}

	/// <summary>Moves the building ghost up and down</summary>
	private void RunUpDownTween()
	{
		spriteRoot.TopLevel = true;

		var upDownTween = CreateTween();
		upDownTween.SetLoops(0);
		upDownTween.TweenProperty(upDownRoot, "position", Vector2.Down * 6, 0.3)
		.SetEase(Tween.EaseType.InOut)
		.SetTrans(Tween.TransitionType.Quad);
		upDownTween.TweenProperty(upDownRoot, "position", Vector2.Up * 6, 0.3)
		.SetEase(Tween.EaseType.InOut)
		.SetTrans(Tween.TransitionType.Quad);
	}
}
