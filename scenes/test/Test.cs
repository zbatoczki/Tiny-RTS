using Godot;

namespace Game.Test;

public partial class Test : Node2D
{
	private Sprite2D sprite2D;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite2D = GetNode<Sprite2D>("Sprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 mousePosition = GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		sprite2D.GlobalPosition = gridPosition.Floor() * 64;
	}
}
