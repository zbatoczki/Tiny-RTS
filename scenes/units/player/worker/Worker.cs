using System.Collections.Generic;
using Game.Resources;
using Godot;
using Game.FSM;
using System.Linq;
using Game.Buildings;
using Game.Globals;
using ResourceManager = Game.Autoload.ResourceManager;
using Game.Groups;
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
	public ResourceNode GatheringResourceTarget {get; set{ field = value; GD.Print(GatheringResourceTarget);}}
	public Building ConstructionTarget {get; set;}

	private Area2D resourceDetector;
	private Vector2? castleLocation;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		
		resourceDetector = GetNode<Area2D>("ResourceDetector");
		resourceDetector.BodyEntered += OnResourceEntered;

		var bases = GetTree().GetNodesInGroup(GlobalGroups.BASE);
		if (bases.Count != 0 && bases.First() is Building castle)
		{
			castleLocation = castle.CenterPosition;
		}

		var treeMaplayer = GetTree().GetNodesInGroup(GlobalGroups.TREE_MAP_LAYER);
		if (treeMaplayer.Count != 0)
		{
			treeLayer = treeMaplayer.First() as TreeTileMapLayerManager;
		}
	}

    private void OnResourceEntered(Node2D body)
    {
		if(body is Castle)
		{
			DropOffResources();
			return;
		}

        if (body is ResourceNode node && !node.IsDepleted && node == GatheringResourceTarget)
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
		if(!HasInventory) return;

		foreach(KeyValuePair<ResourceType, int> resource in CurrentInventory)
		{
			ResourceType resourceType = resource.Key;
			int amount = resource.Value;
			ResourceManager.Instance.AddResource(stats.Faction, resourceType, amount);
		}
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
			MoveTo(GatheringResourceTarget.CenterPosition);
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

	public void Construct()
	{
		if(!IsInstanceValid(ConstructionTarget))
			return;

		ConstructionTarget.Construct(stats.RepairRate);
	}

	public void OnBuldingConstructed(Building constructedBuilding)
	{
		if(IsInstanceValid(ConstructionTarget))
			ConstructionTarget.BuildingConstructed -= OnBuldingConstructed;
		stateMachine.ForceToState<Idle>();
	}

}
