using Game.Manager;
using Game.Resources.Building;
using Godot;
using System;

public partial class BuildingManager : Node
{
	[Export] private SelectionWindow selectionWindow;
	[Export] private TileMapLayer tileMapLayer;

	private enum BuildState
	{
		Idle,
		PlacingBuilding
	}

	private BuildState currentBuildState = BuildState.Idle;
	private BuildingResource buildingToPlace;
	private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);
	private Vector2 buildingGhostDimensions;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		selectionWindow.BuildRequested += OnBuildRequested;
	}

	public override void _UnhandledInput(InputEvent @event)
	{

	}


    public override void _Process(double _)
	{
		var gridPosition = GridManager.WorldPositionToGridCell(tileMapLayer.GetGlobalMousePosition());
		
	}

	private void OnBuildRequested(BuildingResource building)
    {
        ChangeState(BuildState.PlacingBuilding);
        buildingToPlace = building;
    }

	private void ChangeState(BuildState newState)
	{
		if(currentBuildState == BuildState.PlacingBuilding)
		{
			//clear ghostbuilding
			buildingToPlace = null;
		}

		currentBuildState = newState;

		if(currentBuildState == BuildState.PlacingBuilding)
		{
			//instantiate ghostbuilding
			//add to scene
		}
	}


}
