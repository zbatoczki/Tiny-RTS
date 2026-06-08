using Game.Manager;
using Game.Resources.Building;
using Godot;
using Game.InputMap;
using System;
using Game.Autoload;
using Game.Globals;

public partial class BuildingManager : Node
{
	[Export] private SelectionWindow selectionWindow;
	[Export] private TileMapLayer tileMapLayer;
	[Export] private PackedScene buildingGhostScene;

	private enum BuildState
	{
		Idle,
		PlacingBuilding
	}

	private BuildState currentBuildState = BuildState.Idle;
	private BuildingResource toPlaceBuildingResource;
	private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);

	private BuildingGhost buildingGhost;
	private Vector2 buildingGhostDimensions;
	private GridManager gridManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gridManager = GameManager.Instance.Grid;
		selectionWindow.BuildRequested += OnBuildRequested;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		switch (currentBuildState)
		{
			case BuildState.Idle:
				break;
			case BuildState.PlacingBuilding:
				if (evt.IsActionPressed(InputMapping.RIGHT_CLICK))
				{
					ChangeState(BuildState.Idle);
					GetViewport().SetInputAsHandled();
				}
				else if (evt.IsActionPressed(InputMapping.LEFT_CLICK) && toPlaceBuildingResource != null)
				{
					PlaceBuildingAtHoveredCellPosition();
					GetViewport().SetInputAsHandled();
				}
				break;
			default:
				break;
		}
	}

    private void PlaceBuildingAtHoveredCellPosition()
    {
		if(toPlaceBuildingResource.BuildingScene == null)
		{
			GD.PushError($"Building resource {toPlaceBuildingResource.Name} does not have a building scene assigned.");
			return;
		}

        Node2D building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		GetParent().AddChild(building);
		building.GlobalPosition = hoveredGridArea.Position  * GlobalValues.CELL_SIZE;
		ChangeState(BuildState.Idle);
		//TODO: subtract resources, add building to grid manager
    }

    public override void _Process(double _)
	{
		var mouseGridPosition = GridManager.WorldPositionToGridCell(tileMapLayer.GetGlobalMousePosition());
		switch (currentBuildState)
		{
			case BuildState.Idle:
				break;
			case BuildState.PlacingBuilding:
				mouseGridPosition = gridManager.GetMouseGridCellPositionWithDimensionOffset(buildingGhostDimensions, tileMapLayer.GetGlobalMousePosition());
				buildingGhost.GlobalPosition = mouseGridPosition * GlobalValues.CELL_SIZE;
				break;
		}

		var rootCell = hoveredGridArea.Position;
		if(rootCell != mouseGridPosition)
		{
			hoveredGridArea.Position = mouseGridPosition;
			UpdateHoveredGridArea();
		}
		
	}

	private void UpdateGridDisplay()
	{
		//if building placable at area, set valid
		if(IsBuildingPlaceableAtArea(hoveredGridArea))
		{
			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInvalid();
		}

		buildingGhost.DoHoverAnimation();
	}

	/// <summary>Refreshes the grid display when the hovered cell changes during placement.</summary>
	private void UpdateHoveredGridArea()
	{
		switch (currentBuildState)
		{
			case BuildState.Idle:
				break;
			case BuildState.PlacingBuilding:
				UpdateGridDisplay();
				break;
		}
	}


	/// <summary>Whether the building being placed fits and is affordable at the given area.</summary>
	private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
	{
		//check if there is room at the grid area and if the player has enough resources to place the building
		return true;
	}

	/// <summary>Clears the highlight overlay and frees the placement ghost.</summary>
	private void ClearBuildingGhost()
	{
		if (IsInstanceValid(buildingGhost))
		{
			buildingGhost.QueueFree();
		}
		buildingGhost = null;
	}


	private void ChangeState(BuildState newState)
	{
		if(currentBuildState == BuildState.PlacingBuilding)
		{
			ClearBuildingGhost();
			toPlaceBuildingResource = null;
		}

		currentBuildState = newState;

		if(currentBuildState == BuildState.PlacingBuilding)
		{
			buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
			buildingGhost.GlobalPosition = tileMapLayer.GetGlobalMousePosition();
			GetParent().AddChild(buildingGhost);
		}
	}

	private void OnBuildRequested(BuildingResource building)
    {
        ChangeState(BuildState.PlacingBuilding);
		hoveredGridArea.Size = building.Dimensions;

		var buildingSprite = building.SpriteScene.Instantiate<Node2D>();
		buildingGhost.AddSpriteNode(buildingSprite);
		buildingGhost.SetMarkerDimensions(building.Dimensions);
		buildingGhostDimensions = building.Dimensions;

		toPlaceBuildingResource = building;
    }


}
