using System;
using System.Collections.Generic;
using Game.Resources.Gathering;
using Godot;

namespace Game.Resources;

public partial class TreeTileMapLayerManager : Node
{	
	private readonly Vector2I DEPLETED_TREE_ATLAS_COORD = new(0,2);

	[Export] public TileMapLayer TreeLayer {get; set;}

	private Dictionary<Vector2I, GatheringResource> treeResources = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeTrees();
	}

	private void InitializeTrees()
	{
		foreach(Vector2I cell in TreeLayer.GetUsedCells())
		{
			var data = new GatheringResource
			{
				ResourceType = GatheringResource.ResourceTypes.WOOD,
				CellCorrdinates = cell	
			};
			data.ResourceDepleted += OnTreeDepleted;
			treeResources[cell] = data;
		}
	}

	public GatheringResource GetTreeAt(Vector2I cell)
	{
		treeResources.TryGetValue(cell, out GatheringResource data);
		return data;
	}

	public GatheringResource GetNearestTree(Vector2 worldPosition, Vector2I? excludeCell = null)
	{
		GatheringResource nearest = null;
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
        return TreeLayer.ToGlobal(TreeLayer.MapToLocal(treeCellPosition));
    }

    private void OnTreeDepleted(Vector2I cellCoordinates)
    {
		TreeLayer.SetCell(cellCoordinates, 1, DEPLETED_TREE_ATLAS_COORD);
		treeResources.Remove(cellCoordinates);
    }

}
