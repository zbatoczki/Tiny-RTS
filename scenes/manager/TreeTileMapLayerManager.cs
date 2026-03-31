using System;
using System.Collections.Generic;
using Game.Resources.Gathering;
using Godot;

namespace Game.Resources;

public partial class TreeTileMapLayerManager : Node
{	
	private readonly Vector2I DEPLETED_TREE = new(0,2);

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
				Name = "wood",
				CellCorrdinates = cell	
			};
			data.ResourceDepleted += OnTreeDepleted;
			treeResources[cell] = data;
		}
		GD.Print($"Initialized {treeResources.Count} trees.");
	}

	public GatheringResource GetTreeAt(Vector2I cell)
	{
		treeResources.TryGetValue(cell, out GatheringResource data);
		return data;
	}

    private void OnTreeDepleted(Vector2I cellCoordinates)
    {
        GD.Print($"Tree depleted at {cellCoordinates}");
		TreeLayer.SetCell(cellCoordinates, 1, DEPLETED_TREE);
		treeResources.Remove(cellCoordinates);
    }

}
