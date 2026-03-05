using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private Vector2I? hoveredGridCell;
	private Node2D ySortRoot;

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/buildings/Building.tscn");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		ySortRoot = GetNode<Node2D>("YSortRoot");

		ySortRoot.YSortEnabled = true;

		cursor.Visible = false;

		placeBuildingButton = GetNode<Button>("Button");
		placeBuildingButton.Pressed += OnButtonPressed;
	}

    public override void _UnhandledInput(InputEvent evt)
    {
		if (hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePositionValid(hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			cursor.Visible = false;
		}
    }

	public override void _Process(double _)
	{
		Vector2I gridPosition = gridManager.GetMouseGridCellPosition();
		cursor.GlobalPosition =  gridPosition * 64;
		if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			gridManager.HighlightBuildableTiles();
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if(!hoveredGridCell.HasValue) return;

		Node2D buildingInstance = buildingScene.Instantiate<Node2D>();
		buildingInstance.GlobalPosition = hoveredGridCell.Value * 64;
		ySortRoot.AddChild(buildingInstance);

		gridManager.MarkTileAsOccupied(hoveredGridCell.Value);

		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
	}
}
