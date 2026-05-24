using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Resources.Gathering;
using Godot;

namespace Game.Resources;

public partial class TreeTileMapLayerManager : TileMapLayer
{	
	private readonly Vector2I DEPLETED_TREE_ATLAS_COORD = new(0,2);

	//[Export] public TileMapLayer TreeLayer {get; set;}

	private Dictionary<Vector2I, Tree> treeResources = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CallDeferred(MethodName.InitializeTrees);
	}

	private void InitializeTrees()
	{
		var trees = GetChildren().Cast<Tree>();
		var usedCells = GetUsedCells();
		GD.Print("Used cells in tree layer: " + usedCells.Count);
		foreach(Vector2I cell in GetUsedCells())
		{
			var localPos = MapToLocal(cell);

			var treeInstance = trees.FirstOrDefault(tree => tree.Position == localPos);
			if (treeInstance == null)
			{
				GD.PrintErr($"No scene instance found at cell {cell}");
				continue;
			}
			treeInstance.ResourceDepleted += OnTreeDepleted;
			treeInstance.CellCoordinates = cell;
			treeResources[cell] = treeInstance;
		}
		
	}

	public Tree GetTreeAt(Vector2I cell)
	{
		treeResources.TryGetValue(cell, out Tree data);
		return data;
	}

	public Tree GetNearestTree(Vector2 worldPosition, Vector2I? excludeCell = null)
	{
		Tree nearest = null;
		float nearestDist = float.MaxValue;

		foreach (var kvp in treeResources)
		{
			if (kvp.Value.IsDepleted) continue;
			if (excludeCell.HasValue && kvp.Key == excludeCell.Value) continue;

			Vector2 tileWorld = GetGlobalPosition(kvp.Key);
			float dist = worldPosition.DistanceTo(tileWorld);

			if (dist < nearestDist)
			{
				nearestDist = dist;
				nearest = kvp.Value;
			}
		}

		return nearest;
	}

	public Vector2 GetGlobalPosition(Vector2I treeCellPosition)
    {
        return ToGlobal(MapToLocal(treeCellPosition));
    }

    private void OnTreeDepleted(Vector2I cellCoordinates)
    {
		SetCell(cellCoordinates, 1, DEPLETED_TREE_ATLAS_COORD);
		var treeInstance = treeResources[cellCoordinates];
		GameManager.Instance.Grid?.UnregisterTree(treeInstance);
		treeResources.Remove(cellCoordinates);
		treeInstance.QueueFree();
    }

}
