using Game.Manager;
using Game.Resources.Building;
using Godot;
using Game.InputMap;
using Game.Autoload;
using Game.Globals;
using Game.Buildings;
using System.Collections.Generic;

public partial class BuildingManager : Node
{
	[Signal] public delegate void BuildingPlacedEventHandler(Building placedBuilding);

	[Export] private SelectionWindow selectionWindow;
	[Export] private TileMapLayer tileMapLayer;
	[Export] private PackedScene buildingGhostScene;

	private enum BuildState
	{
		Idle,
		PlacingBuilding
	}

	private BuildState currentBuildState = BuildState.Idle;
	private BuildingResource buildingToPlaceResource;
	private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);

	private BuildingGhost buildingGhost;
	private Vector2 buildingGhostDimensions;

	private GridManager gridManager;
	private ResourceManager resourceManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gridManager = GameManager.Instance.Grid;
		resourceManager = ResourceManager.Instance;
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
				else if (evt.IsActionPressed(InputMapping.LEFT_CLICK) && buildingToPlaceResource != null)
				{
					PlaceBuildingAtHoveredCellPosition();
					GetViewport().SetInputAsHandled();
				}
				break;
			default:
				break;
		}
	}

    public override void _Process(double _)
	{
		var mouseGridPosition = GridManager.WorldPositionToGridCell(tileMapLayer.GetGlobalMousePosition());
		if(!gridManager.IsCellWithinBounds(mouseGridPosition.X, mouseGridPosition.Y)) return;
		switch (currentBuildState)
		{
			case BuildState.Idle:
				break;
			case BuildState.PlacingBuilding:
				mouseGridPosition = gridManager.GetMouseGridCellPositionWithDimensionOffset(buildingGhostDimensions, tileMapLayer.GetGlobalMousePosition());
				buildingGhost.GlobalPosition = mouseGridPosition * GlobalValues.CELL_SIZE;
				break;
		}

		if(hoveredGridArea.Position != mouseGridPosition)
		{
			hoveredGridArea.Position = mouseGridPosition;
			UpdateHoveredGridArea();
		}
		
	}

	private void UpdateGridDisplay()
	{
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
		return gridManager.IsCellAreaBuildable(tileArea);
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
			buildingToPlaceResource = null;
		}

		currentBuildState = newState;

		if(currentBuildState == BuildState.PlacingBuilding)
		{
			buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
			var mouseGridPosition = gridManager.GetMouseGridCellPositionWithDimensionOffset(buildingGhostDimensions, tileMapLayer.GetGlobalMousePosition());
			buildingGhost.GlobalPosition = mouseGridPosition * GlobalValues.CELL_SIZE;
			GetParent().AddChild(buildingGhost);
		}
	}

	private void OnBuildRequested(BuildingResource building)
    {
        ChangeState(BuildState.PlacingBuilding);
		hoveredGridArea.Size = building.Dimensions;

		var buildingSprite = building.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddSpriteNode(buildingSprite);
		buildingGhost.SetMarkerDimensions(building.Dimensions);
		buildingGhostDimensions = building.Dimensions;

		buildingToPlaceResource = building;
    }

	private void PlaceBuildingAtHoveredCellPosition()
    {
		if(buildingToPlaceResource.BuildingScene == null)
		{
			GD.PushError($"Building resource {buildingToPlaceResource.Name} does not have a building scene assigned.");
			return;
		}

		FactionType faction = buildingToPlaceResource.Faction;
		var costs = buildingToPlaceResource.ResourceCosts;
		var woodCost = costs.GetValueOrDefault(ResourceType.Wood);
		var foodCost = costs.GetValueOrDefault(ResourceType.Food);
		var goldCost = costs.GetValueOrDefault(ResourceType.Gold);

		if(!resourceManager.CanAfford(faction, woodCost, goldCost, foodCost))
		{
			//TODO: Notify player on screen that there are not enough resources
			GD.PushWarning("Not enough resources.");
			return;
		}

		if (!IsBuildingPlaceableAtArea(hoveredGridArea))
		{
			//TODO: Notify player on screen of invalid placement
			GD.PushWarning("You can't place a building there.");
			return;
		}

        Building building = buildingToPlaceResource.BuildingScene.Instantiate<Building>();
		building.PlacedAtRuntime = true;
		building.BuildingResource = buildingToPlaceResource;
		building.GlobalPosition = hoveredGridArea.Position  * GlobalValues.CELL_SIZE;

		GetParent().AddChild(building);

		resourceManager.Spend(faction, woodCost, goldCost, foodCost);

		EmitSignal(SignalName.BuildingPlaced, building);

		ChangeState(BuildState.Idle);
    }


}
