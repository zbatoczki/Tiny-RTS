using System.Linq;
using Game.Buildings;
using Godot;
using Tree = Game.Resources.Tree;

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

	public void DebugPrintGrid()
	{
		var sb = new System.Text.StringBuilder();
		sb.Append($"GridManager {GridWidth}x{GridHeight} — legend: . empty, U unit, R resource, B building\n");

		sb.Append("   ");
		for (int x = 0; x < GridWidth; x++)
		{
			sb.Append(x % 10);
		}
		sb.Append('\n');

		for (int y = 0; y < GridHeight; y++)
		{
			sb.Append($"{y,3} ");
			for (int x = 0; x < GridWidth; x++)
			{
				sb.Append(grid[x, y].cellState switch
				{
					CellState.Empty    => '.',
					CellState.Unit     => 'U',
					CellState.Resource => 'R',
					CellState.Building => 'B',
					_                  => '?',
				});
			}
			sb.Append('\n');
		}

		GD.Print(sb.ToString());

		for (int y = 0; y < GridHeight; y++)
		{
			for (int x = 0; x < GridWidth; x++)
			{
				var (state, node) = grid[x, y];
				if (state == CellState.Empty) continue;
				GD.Print($"  ({x},{y}) {state} -> {node?.Name} [{node?.GetType().Name}]");
			}
		}
	}

	public void UnregisterTree(Tree tree)
	{
		var anchorCell = tree.CellCoordinates;
		var dims = tree.Dimensions;

		for (int dx = 0; dx < dims.X; dx++)
		{
			for (int dy = 0; dy < dims.Y; dy++)
			{
				int x = anchorCell.X - 1 - dx;
				int y = anchorCell.Y - 1 - dy;

				if (!IsCellWithinBounds(x, y)) continue;
				if (grid[x, y].node != tree) continue;

				grid[x, y] = (CellState.Empty, null);
			}
		}
	}

	public void RegisterBuilding(Building building)
	{
		var dims = building.BuildingResource?.Dimensions ?? Vector2I.One;
		var anchorCell = WorldPositionToGridCell(building.GlobalPosition);
		int ax = (int)anchorCell.X;
		int ay = (int)anchorCell.Y;

		for (int dx = 0; dx < dims.X; dx++)
		{
			for (int dy = 0; dy < dims.Y; dy++)
			{
				int x = ax + dx;
				int y = ay + dy;

				if (!IsCellWithinBounds(x, y)) continue;

				grid[x, y] = (CellState.Building, building);
			}
		}
	}

	public void UnregisterBuilding(Building building)
	{
		var dims = building.BuildingResource?.Dimensions ?? Vector2I.One;
		var anchorCell = WorldPositionToGridCell(building.GlobalPosition);
		int ax = (int)anchorCell.X;
		int ay = (int)anchorCell.Y;

		for (int dx = 0; dx < dims.X; dx++)
		{
			for (int dy = 0; dy < dims.Y; dy++)
			{
				int x = ax + dx;
				int y = ay + dy;

				if (!IsCellWithinBounds(x, y)) continue;
				if (grid[x, y].node != building) continue;

				grid[x, y] = (CellState.Empty, null);
			}
		}
	}

	public void RegisterTrees(TileMapLayer treeLayer)
	{
		GD.Print(treeLayer.GetChildCount());
		foreach (var tree in treeLayer.GetChildren().OfType<Tree>())
		{
			var anchorCell = treeLayer.LocalToMap(tree.Position);
			var dims = tree.Dimensions;
			GD.Print($"Registering tree {tree.Name} at cell {anchorCell} with dims {dims}");
			for (int dx = 0; dx < dims.X; dx++)
			{
				for (int dy = 0; dy < dims.Y; dy++)
				{
					int x = anchorCell.X - 1 - dx;
					int y = anchorCell.Y - 1 - dy;

					if (!IsCellWithinBounds(x, y)) continue;

					grid[x, y] = (CellState.Resource, tree);
				}
			}
		}
	}
}
