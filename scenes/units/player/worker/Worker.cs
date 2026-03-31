
using System;
using System.Collections.Generic;
using Game.Resources;
using Game.Resources.Gathering;
using Godot;

namespace Game.Units;

public partial class Worker : MeleeUnit
{
	[Export] private TreeTileMapLayerManager treeManager;
	[Export] private float GatheringRange;

	public Dictionary<string, int> CurrentInventory = new()
	{
		{"Wood", 0},
		{"Gold", 0},
		{"Food", 0}
	};

	private Area2D resourceDetector;
	public GatheringResource GatheringResourceTarget {get; set;}

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

}
