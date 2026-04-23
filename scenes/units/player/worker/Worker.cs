using System.Collections.Generic;
using Game.Resources;
using Godot;
using Game.FSM;
using System.Linq;
using Game.Resources.Gathering;
using Game.Autoload;
using Game.Buildings;
using Game.Globals;

namespace Game.Units;

public partial class Worker : MeleeUnit
{
	[Export] private TreeTileMapLayerManager treeLayer;
	[Export] private float GatheringRange;

	public Dictionary<ResourceType, int> CurrentInventory = new()
	{
		{ResourceType.Wood, 0},
		{ResourceType.Gold, 0},
		{ResourceType.Food, 0}
	};

	public bool HasInventory => CurrentInventory.Any(resource => resource.Value > 0);
	public ResourceNode GatheringResourceTarget {get; set;}

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
		var node = body as ResourceNode;
		GD.Print(node + ":" + GatheringResourceTarget);
        if (node != null && !node.IsDepleted && node == GatheringResourceTarget)
        {
            if (node is GoldMine goldmine)
			{
                goldmine.EnterMine(this);
			}
            stateMachine.ForceToState<Gather>();
        }
    }

	public void DropOffResources()
	{
		ResourceEvents.EmitResourcesModified(
			CurrentInventory[ResourceType.Wood], 
			CurrentInventory[ResourceType.Gold], 
			CurrentInventory[ResourceType.Food]
		);
		ClearAllInventoryCounts();
		ReturnToResource();
	}

	public void ReturnToCastle()
	{
		if(!castleLocation.HasValue) return;

		MoveTo(castleLocation.Value);
	}

	public void PrintCurrentInventory()
	{
		foreach(var key in CurrentInventory.Keys)
		{
			GD.Print($"{key}: {CurrentInventory[key]}");
		}
	}

	private void ClearAllInventoryCounts()
	{
		foreach(var key in CurrentInventory.Keys)
		{
			CurrentInventory[key] = 0;
		}
	}

	private void ReturnToResource()
	{
		
		if(GatheringResourceTarget.Type == ResourceType.Gold && !GatheringResourceTarget.IsDepleted)
		{
			MoveTo(GatheringResourceTarget.CellCoordinates);
		}
		else if(GatheringResourceTarget.Type == ResourceType.Wood)
		{
			Vector2I lastTreeCellPosition = GatheringResourceTarget.CellCoordinates;
	
			if (!GatheringResourceTarget.IsDepleted)
			{
				MoveTo(treeLayer.GetGlobalPosition(GatheringResourceTarget.CellCoordinates));
				return;
			}

			Vector2 lastTreeWorldPosition = treeLayer.ToGlobal(treeLayer.MapToLocal(lastTreeCellPosition));
			Resources.Tree nearestTree = treeLayer.GetNearestTree(lastTreeWorldPosition, lastTreeCellPosition);
			GatheringResourceTarget = nearestTree;
			if(nearestTree != null)
			{
				MoveTo(treeLayer.GetGlobalPosition(GatheringResourceTarget.CellCoordinates));
			}
			else
			{
				stateMachine.ForceToState<Idle>();
			}
		}
		else
		{
			stateMachine.ForceToState<Idle>();
		}
	}

}
