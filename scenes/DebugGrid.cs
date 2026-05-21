using Godot;

/// <summary>
/// Debug overlay that draws a 64x64 world-space grid matching GridManager cells.
/// Toggle at runtime with the configured key. Also renders live in the editor.
/// </summary>
[Tool]
public partial class DebugGrid : Node2D
{
	[Export] public int CellSize = 64;
	[Export] public int Columns = 32;
	[Export] public int Rows = 24;
	[Export] public Color LineColor = new(1f, 1f, 1f, 0.25f);
	[Export] public bool ShowCoordinates = false;
	[Export] public Key ToggleKey = Key.G;

	public override void _Ready()
	{
		// Draw above terrain/units; UI lives on a separate CanvasLayer so it stays on top.
		ZIndex = 1000;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true, Echo: false } key && key.Keycode == ToggleKey)
		{
			Visible = !Visible;
		}
	}

	public override void _Draw()
	{
		int width = Columns * CellSize;
		int height = Rows * CellSize;

		for (int x = 0; x <= Columns; x++)
		{
			float px = x * CellSize;
			DrawLine(new Vector2(px, 0), new Vector2(px, height), LineColor);
		}

		for (int y = 0; y <= Rows; y++)
		{
			float py = y * CellSize;
			DrawLine(new Vector2(0, py), new Vector2(width, py), LineColor);
		}

		if (ShowCoordinates)
		{
			var font = ThemeDB.FallbackFont;
			for (int x = 0; x < Columns; x++)
			{
				for (int y = 0; y < Rows; y++)
				{
					var pos = new Vector2(x * CellSize + 3, y * CellSize + 13);
					DrawString(font, pos, $"{x},{y}", HorizontalAlignment.Left, -1, 11, LineColor);
				}
			}
		}
	}
}
