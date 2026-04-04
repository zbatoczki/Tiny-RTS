using System.Collections.Generic;
using Game.Resources;
using Godot;
using Game.FSM;
using System.Linq;
using Game.Resources.Gathering;
using Game.Autoload;

namespace Game.Units;

public partial class Worker : MeleeUnit
{
	[Export] private TreeTileMapLayerManager treeManager;
	[Export] private float GatheringRange;

	public Dictionary<string, int> CurrentInventory = new()
	{
		{"wood", 0},
		{"gold", 0},
		{"food", 0}
	};

	public bool HasInventory => CurrentInventory.Any(resource => resource.Value > 0);
	public GatheringResource GatheringResourceTarget {get; set;}

	private Area2D resourceDetector;
	private Vector2? castleLocation;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		
		resourceDetector = GetNode<Area2D>("ResourceDetector");
		resourceDetector.BodyEntered += OnResourceEntered;

		var bases = GetTree().GetNodesInGroup("Base");
		if (bases.Count != 0)
		{
			castleLocation = (bases.First() as Node2D).GlobalPosition;
		}
	}

    private void OnResourceEntered(Node2D body)
    {
		GD.Print("Resource detected");
        stateMachine.ForceToState<Gather>();
    }

	public void DropOffResources()
	{
		//TODO: call ResourceEventBus and pass resource counts
		ResourceEvents.EmitResourcesModified(CurrentInventory["wood"], CurrentInventory["gold"], CurrentInventory["food"]);

		Vector2I lastTreeCellPosition = GatheringResourceTarget.CellCorrdinates;
		ClearAllInventoryCounts();
		GatheringResourceTarget = null;

		Vector2 lastTreeWorldPosition = treeManager.TreeLayer.ToGlobal(treeManager.TreeLayer.MapToLocal(lastTreeCellPosition));

		GatheringResource nearestTree = treeManager.GetNearestTree(lastTreeWorldPosition, lastTreeCellPosition);
		if(nearestTree != null)
		{
			GatheringResourceTarget = nearestTree;
			MoveTo(treeManager.GetGlobalPosition(GatheringResourceTarget.CellCorrdinates));
		}
		else
		{
			stateMachine.ForceToState<Idle>();
		}

	}

	public void ReturnToCastle()
	{
		if(!castleLocation.HasValue) return;

		MoveTo(castleLocation.Value);
	}

	private void ClearAllInventoryCounts()
	{
		foreach(var key in CurrentInventory.Keys)
		{
			CurrentInventory[key] = 0;
		}
	}

	private void PrintCurrentInventory()
	{
		foreach(var key in CurrentInventory.Keys)
		{
			GD.Print($"{key}: {CurrentInventory[key]}");
		}
	}

}
