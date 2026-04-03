using System.Collections.Generic;
using Game.Resources;
using Godot;
using Game.FSM;
using System.Linq;

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

	private Area2D resourceDetector;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		
		resourceDetector = GetNode<Area2D>("ResourceDetector");
		resourceDetector.BodyEntered += OnResourceEntered;
	}

    private void OnResourceEntered(Node2D body)
    {
		GD.Print("Resource detected");
        stateMachine.ForceToState<Gather>();
    }

	public void DropOffResources()
	{
		//TODO: call ResourceEventBus and pass resource counts
		GD.Print("Dropping off Resources");
		ClearAllInventoryCounts();
		stateMachine.ForceToState<Idle>();
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
