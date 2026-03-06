using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;
	private PackedScene towerScene;
	private PackedScene houseScene;
	private Button placeTowerButton;
	private Button placeHouseButton;
	private Vector2I? hoveredGridCell;
	private Node2D ySortRoot;
	private PackedScene toPlaceBuildingScene;

	public override void _Ready()
	{
		towerScene = GD.Load<PackedScene>("res://scenes/buildings/Tower.tscn");
		houseScene = GD.Load<PackedScene>("res://scenes/buildings/House.tscn");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		ySortRoot = GetNode<Node2D>("YSortRoot");

		ySortRoot.YSortEnabled = true;

		cursor.Visible = false;

		placeTowerButton = GetNode<Button>("PlaceTowerButton");
		placeHouseButton = GetNode<Button>("PlaceHouseButton");
		placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
		placeHouseButton.Pressed += OnPlaceHouseButtonPressed;
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

		Node2D buildingInstance = toPlaceBuildingScene.Instantiate<Node2D>();
		buildingInstance.GlobalPosition = hoveredGridCell.Value * 64;
		ySortRoot.AddChild(buildingInstance);

		gridManager.MarkTileAsOccupied(hoveredGridCell.Value);

		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
	}

	private void OnPlaceTowerButtonPressed()
	{
		toPlaceBuildingScene = towerScene;
		cursor.Visible = true;
	}

	private void OnPlaceHouseButtonPressed()
	{
		toPlaceBuildingScene = houseScene;
		cursor.Visible = true;

	}
}
