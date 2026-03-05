using System.Collections.Generic;
using Godot;

namespace Game;

public partial class Main : Node
{
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private TileMapLayer highlightTileMaplayer;
	
	private Button placeBuildingButton;

	private Vector2? hoveredGridCell;

	private HashSet<Vector2> occupiedCells = [];

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/buildings/Building.tscn");

		cursor = GetNode<Sprite2D>("Cursor");
		cursor.Visible = false;

		highlightTileMaplayer = GetNode<TileMapLayer>("HighlightTilemapLayer");
		placeBuildingButton = GetNode<Button>("Button");
		placeBuildingButton.Pressed += OnButtonPressed;
	}

    public override void _UnhandledInput(InputEvent evt)
    {
		if (hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && !occupiedCells.Contains(hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			cursor.Visible = false;
		}
    }

	public override void _Process(double _)
	{
		Vector2 gridPosition = GetMouseGridCellPosition();
		cursor.GlobalPosition =  gridPosition * 64;
		if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			UpdateHighlightTilemapLayer(3);
		}
	}

	private void UpdateHighlightTilemapLayer(int radius = 0)
	{
		highlightTileMaplayer.Clear();
		if (!hoveredGridCell.HasValue)
		{
			return;
		}

		for (float x = hoveredGridCell.Value.X - radius; x <= hoveredGridCell.Value.X + radius; x++)
		{
			for (float y = hoveredGridCell.Value.Y - radius; y <= hoveredGridCell.Value.Y + radius; y++)
			{
				highlightTileMaplayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
			}
		}

	} 

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if(!hoveredGridCell.HasValue) return;

		Node2D buildingInstance = buildingScene.Instantiate<Node2D>();
		buildingInstance.GlobalPosition = hoveredGridCell.Value * 64;
		AddChild(buildingInstance);

		occupiedCells.Add(hoveredGridCell.Value);

		hoveredGridCell = null;
		UpdateHighlightTilemapLayer();
	}

	private Vector2 GetMouseGridCellPosition()
	{
		Vector2 mousePosition = highlightTileMaplayer.GetGlobalMousePosition();
		return (mousePosition / 64).Floor();
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
	}
}
