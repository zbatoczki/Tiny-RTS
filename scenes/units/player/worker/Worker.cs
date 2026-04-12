using System.Collections.Generic;
using Game.Resources;
using Godot;
using Game.FSM;
using System.Linq;
using Game.Resources.Gathering;
using Game.Autoload;
using Game.Buildings;

namespace Game.Units;

public partial class Worker : MeleeUnit
{
	[Export] private TreeTileMapLayerManager treeManager;
	[Export] private float GatheringRange;

	public Dictionary<GatheringResource.ResourceTypes, int> CurrentInventory = new()
	{
		{GatheringResource.ResourceTypes.WOOD, 0},
		{GatheringResource.ResourceTypes.GOLD, 0},
		{GatheringResource.ResourceTypes.FOOD, 0}
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
		if (body is GoldMine goldmine && !goldmine._GatheringResource.IsDepleted)
		{
			goldmine.EnterMine(this);
		}
        stateMachine.ForceToState<Gather>();
    }

	public void DropOffResources()
	{
		//TODO: call ResourceEventBus and pass resource counts
		ResourceEvents.EmitResourcesModified(CurrentInventory[GatheringResource.ResourceTypes.WOOD], CurrentInventory[GatheringResource.ResourceTypes.GOLD], CurrentInventory[GatheringResource.ResourceTypes.FOOD]);
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
		if(GatheringResourceTarget == null || GatheringResourceTarget.IsDepleted)
		{
			stateMachine.ForceToState<Idle>();
			return;
		}

		if(GatheringResourceTarget.ResourceType == GatheringResource.ResourceTypes.GOLD)
		{
			MoveTo(GatheringResourceTarget.CellCorrdinates);
		}
		else if(GatheringResourceTarget.ResourceType == GatheringResource.ResourceTypes.WOOD)
		{
			Vector2I lastTreeCellPosition = GatheringResourceTarget.CellCorrdinates;
	
			if (!GatheringResourceTarget.IsDepleted)
			{
				MoveTo(treeManager.GetGlobalPosition(GatheringResourceTarget.CellCorrdinates));
				return;
			}

			Vector2 lastTreeWorldPosition = treeManager.TreeLayer.ToGlobal(treeManager.TreeLayer.MapToLocal(lastTreeCellPosition));
			GatheringResource nearestTree = treeManager.GetNearestTree(lastTreeWorldPosition, lastTreeCellPosition);
			GatheringResourceTarget = nearestTree;
			if(nearestTree != null)
			{
				MoveTo(treeManager.GetGlobalPosition(GatheringResourceTarget.CellCorrdinates));
			}
			else
			{
				stateMachine.ForceToState<Idle>();
			}
		}
	}

}
