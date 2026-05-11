using Godot;
using System;

public partial class Camera2d : Camera2D
{
	[Export] public float MoveSpeed        = 600f;
    [Export] public float EdgeScrollMargin = 20f;
    [Export] public float ZoomStep         = 0.1f;
    [Export] public float MinZoom          = 0.4f;
    [Export] public float MaxZoom          = 2.0f;
    [Export] public Rect2 MapBounds        = new(0, 0, 3200, 3200);

    public override void _Process(double delta)
    {
        var move   = Vector2.Zero;
        var mouse  = GetViewport().GetMousePosition();
        var vpSize = GetViewport().GetVisibleRect().Size;

        if (Input.IsActionPressed("ui_left")  || mouse.X < EdgeScrollMargin)
			move.X -= 1;
        if (Input.IsActionPressed("ui_right") || mouse.X > vpSize.X - EdgeScrollMargin) 
			move.X += 1;
        if (Input.IsActionPressed("ui_up")    || mouse.Y < EdgeScrollMargin)
			move.Y -= 1;
        if (Input.IsActionPressed("ui_down")  || mouse.Y > vpSize.Y - EdgeScrollMargin)
			move.Y += 1;

        Position += move.Normalized() * MoveSpeed * (float)delta / Zoom.X;
        Position  = new Vector2(
            Mathf.Clamp(Position.X, MapBounds.Position.X, MapBounds.End.X),
            Mathf.Clamp(Position.Y, MapBounds.Position.Y, MapBounds.End.Y));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.WheelUp })
            Zoom = (Zoom + new Vector2(ZoomStep, ZoomStep)).Clamp(
                new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
				
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.WheelDown })
            Zoom = (Zoom - new Vector2(ZoomStep, ZoomStep)).Clamp(
                new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
	}
}
