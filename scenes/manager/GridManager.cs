using System.Collections.Generic;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	private HashSet<Vector2I> occupiedCells = [];

	[Export]
	private TileMapLayer highlightTilemapLayer;
	[Export]
	private TileMapLayer baseTerrainTilemapLayer;

	public override void _Ready()
	{
	}

	public bool IsTilePositionValid(Vector2I tilePosition)
	{
		var customData = baseTerrainTilemapLayer.GetCellTileData(tilePosition);

		if(customData == null || !(bool)customData.GetCustomData("buildable"))
		{
			return false;
		}

		return !occupiedCells.Contains(tilePosition);
	}

	public void MarkTileAsOccupied(Vector2I tilePosition)
	{
		occupiedCells.Add(tilePosition);
	}

	public void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
	{
		ClearHighlightedTiles();

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

	public void ClearHighlightedTiles()
	{
		highlightTilemapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPosition()
	{
		Vector2 mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
		var gridPosition = (mousePosition / 64).Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

}
