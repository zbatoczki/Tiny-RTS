using System.Linq;
using Game.Buildings;
using Godot;
using Game.Globals;
using Tree = Game.Resources.Tree;
using System.Collections.Generic;

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

	public int GridWidth { get; private set; }
	public int GridHeight { get; private set; }

	private readonly (CellState cellState, Node2D node)[,] grid;
	private readonly AStarGrid2D pathfinder;

	public GridManager(int mapWidth, int mapHeight)
	{
		GridWidth = mapWidth;
		GridHeight = mapHeight;
		grid = new (CellState, Node2D)[GridWidth, GridHeight];
		InitializeGrid();

		pathfinder = new AStarGrid2D
		{
			Region = new Rect2I(0, 0, GridWidth, GridHeight),
			CellSize = new Vector2(GlobalValues.CELL_SIZE, GlobalValues.CELL_SIZE),
			DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles,
		};
		pathfinder.Update();
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

	private void SetCell(int x, int y, CellState state, Node2D node)
	{
		grid[x, y] = (state, node);
		pathfinder?.SetPointSolid(new Vector2I(x, y), state != CellState.Empty);
	}

	public Vector2[] FindPath(Vector2 worldFrom, Vector2 worldTo)
	{
		var fromV = WorldPositionToGridCell(worldFrom);
		var toV = WorldPositionToGridCell(worldTo);
		var fromCell = new Vector2I(fromV.X, fromV.Y);
		var toCell = new Vector2I(toV.X, toV.Y);

		if (!IsCellWithinBounds(fromCell.X, fromCell.Y) || !IsCellWithinBounds(toCell.X, toCell.Y))
			return [];

		// AStarGrid2D refuses to start from a solid cell; temporarily unmark while querying.
		bool fromWasSolid = pathfinder.IsPointSolid(fromCell);
		if (fromWasSolid) pathfinder.SetPointSolid(fromCell, false);

		var path = pathfinder.GetPointPath(fromCell, toCell, allowPartialPath: true);

		if (fromWasSolid) pathfinder.SetPointSolid(fromCell, true);

		// GetPointPath returns cell origins (top-left). Shift to cell centers for unit consumers.
		var halfCell = new Vector2(GlobalValues.HALF_CELL_SIZE, GlobalValues.HALF_CELL_SIZE);
		for (int i = 0; i < path.Length; i++)
			path[i] += halfCell;

		return path;
	}

	public bool IsCellFree(int x, int y) => IsCellWithinBounds(x, y) && grid[x, y].cellState == CellState.Empty;

	public bool IsCellWithinBounds(int x, int y) => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;

	public static Vector2I WorldPositionToGridCell(Vector2 worldPos)
	{
		Vector2 tilePsotion = (worldPos / GlobalValues.CELL_SIZE).Floor();
		return new Vector2I((int)tilePsotion.X, (int)tilePsotion.Y);
	}

	public bool TryRegisterEntityAtCell(Vector2 cellPos, CellState state, Node2D entityNode)
	{
		var cellPosition = WorldPositionToGridCell(cellPos);
		var x = cellPosition.X;
		var y = cellPosition.Y;

		if(!IsCellFree(x, y))
		{
			return false;
		}

		SetCell(x, y, state, entityNode);
		return true;
	}

	

	// Clears all registered cells. Call after a scene reload so the grid does not
	// retain references to freed (disposed) nodes from the previous scene.
	public void Reset()
	{
		InitializeGrid();
		pathfinder?.FillSolidRegion(pathfinder.Region, false);
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
				if (!GodotObject.IsInstanceValid(node))
				{
					GD.Print($"  ({x},{y}) {state} -> <freed>");
					continue;
				}
				GD.Print($"  ({x},{y}) {state} -> {node.Name} [{node.GetType().Name}]");
			}
		}
	}

	public bool IsCellAreaBuildable(Rect2I tileArea)
	{
		List<Vector2I> cells = tileArea.ToCells();

		if(cells.Count == 0) return false;

		foreach(Vector2I cell in cells)
			if(!IsCellFree(cell.X, cell.Y)) //TODO: Units will need to move when building is placed.
				return false;

		return true;
	}

	public void RegisterBuilding(Building building)
	{
		var dims = building.BuildingResource?.Dimensions ?? Vector2I.One;
		var anchorCell = WorldPositionToGridCell(building.GlobalPosition);
		int ax = anchorCell.X;
		int ay = anchorCell.Y;

		for (int dx = 0; dx < dims.X; dx++)
		{
			for (int dy = 0; dy < dims.Y; dy++)
			{
				int x = ax + dx;
				int y = ay + dy;

				if (!IsCellWithinBounds(x, y)) continue;

				SetCell(x, y, CellState.Building, building);
			}
		}
	}

	public void UnregisterBuilding(Building building)
	{
		var dims = building.BuildingResource?.Dimensions ?? Vector2I.One;
		var anchorCell = WorldPositionToGridCell(building.GlobalPosition);
		int ax = anchorCell.X;
		int ay = anchorCell.Y;

		for (int dx = 0; dx < dims.X; dx++)
		{
			for (int dy = 0; dy < dims.Y; dy++)
			{
				int x = ax + dx;
				int y = ay + dy;

				if (!IsCellWithinBounds(x, y) || grid[x, y].node != building) continue;

				SetCell(x, y, CellState.Empty, null);
			}
		}
	}

	public void RegisterTrees(TileMapLayer treeLayer)
	{
		foreach (var tree in treeLayer.GetChildren().OfType<Tree>())
		{
			var anchorCell = treeLayer.LocalToMap(tree.Position);
			var dims = tree.Dimensions;
			GD.Print($"Registering tree {tree.Name} at cell {anchorCell}/position {tree.Position} with dims {dims}");
			for (int dx = 0; dx < dims.X; dx++)
			{
				for (int dy = 0; dy < dims.Y; dy++)
				{
					int x = anchorCell.X + dx;
					int y = anchorCell.Y - dy;

					if (!IsCellWithinBounds(x, y) || grid[x, y].cellState != CellState.Empty) continue;

					SetCell(x, y, CellState.Resource, tree);
				}
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
				int x = anchorCell.X + dx;
				int y = anchorCell.Y - dy;

				if (!IsCellWithinBounds(x, y) || grid[x, y].node != tree) continue;

				SetCell(x, y, CellState.Empty, null);
			}
		}
	}

	public void RegisterGoldmine(GoldMine goldMine)
	{
		var anchorCell = WorldPositionToGridCell(goldMine.GlobalPosition);
		var dims = goldMine.Dimensions;
		for (int dx = 0; dx < dims.X; dx++)
		{
			for (int dy = 0; dy < dims.Y; dy++)
			{
				int x = anchorCell.X + dx;
				int y = anchorCell.Y + dy;
				if (!IsCellWithinBounds(x, y) || grid[x, y].cellState != CellState.Empty) continue;
				SetCell(x, y, CellState.Resource, goldMine);
			}
		}
	}

    public Vector2I GetMouseGridCellPositionWithDimensionOffset(Vector2 dimensions, Vector2 tileMapGlobalMousePosition)
    {
        Vector2 mouseGridPosition = tileMapGlobalMousePosition / GlobalValues.CELL_SIZE;
		mouseGridPosition -= dimensions / 2;
		mouseGridPosition = mouseGridPosition.Round();
		return new Vector2I((int)mouseGridPosition.X, (int)mouseGridPosition.Y);
    }

	/// <summary>
	/// Finds adjacent cells relative to a root grid cell position and dimensions. Can choose if only open cells are selected.
	/// </summary>
	/// <returns>Returns an enumerable of Vector2I representing adjacent cells</returns>
	public List<Vector2I> GetAdjecentCells(Vector2I rootCell, Vector2I dimension, bool openCellsOnly = false)
	{
		List<Vector2I> result = [];
		if (dimension.X <= 0 || dimension.Y <= 0)
			return result;

		// The footprint spans rootCell (top-left) to rootCell + dimension - 1 (bottom-right).
		int left = rootCell.X - 1;
		int right = rootCell.X + dimension.X;
		int top = rootCell.Y - 1;
		int bottom = rootCell.Y + dimension.Y;

		void TryAdd(int x, int y)
		{
			if (!openCellsOnly || IsCellFree(x, y))
				result.Add(new Vector2I(x, y));
		}

		// Top and bottom rows, including the four corners.
		for (int x = left; x <= right; x++)
		{
			TryAdd(x, top);
			TryAdd(x, bottom);
		}

		// Left and right columns, excluding the corners already added above.
		for (int y = rootCell.Y; y < rootCell.Y + dimension.Y; y++)
		{
			TryAdd(left, top);
			TryAdd(right, top);
		}

		return result;
	}

}
