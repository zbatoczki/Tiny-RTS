using System.Reflection.Metadata.Ecma335;
using Godot;

namespace Game.Manager;

public partial class GridManager
{
	public enum CellState
	{
		Empty,
		Unit,
		Resource,
		Building
	}

	public const int CELL_SIZE = 64;
	public int GridWidth { get; private set; }
	public int GridHeight { get; private set; }

	private (CellState cellState, Node2D node)[,] grid;

	public GridManager(int mapWidth, int mapHeight)
	{
		GridWidth = mapWidth;
		GridHeight = mapHeight;
		grid = new (CellState, Node2D)[GridWidth, GridHeight];
		InitializeGrid();
	}

	public bool IsCellFree(int x, int y) => grid[x, y].cellState == CellState.Empty;

	public bool IsCellWithinBounds(int x, int y) => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;

	public static Vector2 WorldPositionToGridCell(Vector2 worldPos)
	{
		var x = Mathf.Floor(worldPos.X / CELL_SIZE);
		var y = Mathf.Floor(worldPos.Y / CELL_SIZE);
		return new Vector2(x, y);
	}

	public bool TryRegisterEntityAtCell(Vector2 cellPos, CellState state, Node2D entityNode)
	{
		var cellPosition = WorldPositionToGridCell(cellPos);
		var x = (int)cellPosition.X;
		var y = (int)cellPosition.Y;

		if(!IsCellFree(x, y))
		{
			return false;
		}

		grid[x, y] = (state, entityNode);
		return true;
	}

	private void InitializeGrid()
	{
		for (int x = 0; x < GridWidth; x++)
		{
			for (int y = 0; y < GridHeight; y++)
			{
				grid[x, y] = (CellState.Empty, null);
			}
		}
	}
}
