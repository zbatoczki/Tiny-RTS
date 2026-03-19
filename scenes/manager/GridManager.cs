using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	private const int CELL_SIZE = 64;
	
	private HashSet<Vector2I> occupiedCells = [];

	[Export]
	private TileMapLayer highlightTilemapLayer;
	[Export]
	private TileMapLayer baseTerrainTilemapLayer;

	private List<TileMapLayer> allTilemapLayers = [];

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		UnitsEvents.Instance.UnitMovementFinsished += OnUnitMovementFinished;
		UnitsEvents.Instance.UnitMovementStarted += OnUnitMovementStarted;

		allTilemapLayers = GetAllTilemapLayers(baseTerrainTilemapLayer);
	}

	public bool IsTilePositionValid(Vector2I tilePosition)
	{
		foreach(var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);
			if(customData == null) continue;
			return (bool)customData.GetCustomData("buildable");
		}
		return false;
	}

	public void MarkTileAsOccupied(Vector2I tilePosition) => occupiedCells.Add(tilePosition);

	public void MarkTileAsUnoccupied(Vector2I tilePosition) => occupiedCells.Remove(tilePosition);

	public bool IsTileOccupied(Vector2I cellPosition) => occupiedCells.Contains(cellPosition);

	public void HighlightBuildableTiles()
	{
		ClearHighlightedTiles();
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		foreach(var buildingComponent in buildingComponents)
		{
			HighlightValidTilesInRadius(buildingComponent.GetGridCellPosition(), buildingComponent.BuildableRadius);
		}
	}

	public void ClearHighlightedTiles()
	{
		highlightTilemapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPosition()
	{
		Vector2 mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
		return ConvertWorldPositionToTilePosition(mousePosition);
	}

	public static Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
	{
		Vector2 tilePosition = (worldPosition / 64).Floor();
		return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
	}

	private List<TileMapLayer> GetAllTilemapLayers(TileMapLayer rootTilemapLayer)
	{
		var result = new List<TileMapLayer>();
		var children = rootTilemapLayer.GetChildren();
		children.Reverse();
		foreach(var child in children)
		{
			if(child is TileMapLayer childLayer)
			{
				result.AddRange(GetAllTilemapLayers(childLayer));
			}
		}
		result.Add(rootTilemapLayer);
		return result;
	}

	private void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
	{
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				Vector2I tilePosition = new(x, y);
				if(!IsTilePositionValid(tilePosition)) 
				{
					continue;
				}
				highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
			}
		}
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		MarkTileAsOccupied(buildingComponent.GetGridCellPosition());
	}

	private void OnUnitMovementFinished(Vector2 globalPosition)
	{
		MarkTileAsOccupied(ConvertWorldPositionToTilePosition(globalPosition));
	}

	private void OnUnitMovementStarted(Vector2 globalPosition)
	{
		MarkTileAsUnoccupied(ConvertWorldPositionToTilePosition(globalPosition));
	}

}
