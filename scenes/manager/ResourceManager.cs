using System;
using System.Collections.Generic;
using Game.Globals;
using Godot;

namespace Game.Autoload;

public partial class ResourceManager : Node
{
	public static ResourceManager Instance { get; private set; }

	[Signal] public delegate void ResourceChangedEventHandler(int faction, int gold, int wood, int food);

	private Dictionary<FactionType, int> gold = [];
    private Dictionary<FactionType, int> wood = [];
    private Dictionary<FactionType, int> food = [];

	public override void _Ready()
	{
		//ResourceEvents.Instance.ResourcesModified += OnResourcesModified;

		if (Instance != null) 
		{ 
			QueueFree(); 
			return;
		}
        Instance = this;
        InitializeResources();
	}

	public int  GetGold(FactionType f) => gold[f];
    public int  GetWood(FactionType f) => wood[f];
	public int  GetFood(FactionType f) => food[f];

	public bool CanAfford(FactionType faction, int woodCost = 0, int goldCost = 0, int foodCost = 0)
	{
		return 
		gold[faction] >= goldCost && 
		wood[faction] >= woodCost &&
		food[faction] >= foodCost;
	}

	public bool Spend(FactionType faction, int woodCost = 0, int goldCost = 0, int foodCost = 0)
	{
		gold[faction] = Math.Max(0, gold[faction] - goldCost);
		wood[faction] = Math.Max(0, wood[faction] -woodCost);
		food[faction] = Math.Max(0, food[faction] - foodCost);

		EmitSignal(SignalName.ResourceChanged, (int) faction, gold[faction], wood[faction], food[faction]);

		return true;
	}

	public void AddResource(FactionType faction, ResourceType type, int amount)
	{
		if(type == ResourceType.Food)
		{
			food[faction] += amount;
		}
		else if (type == ResourceType.Gold)
		{
			gold[faction] += amount;
		}
		else
		{
			wood[faction] += amount;
		}
		EmitSignal(SignalName.ResourceChanged, (int)faction, gold[faction], wood[faction], food[faction]);
	}

	public void InitializeResources()
	{
		foreach (FactionType f in System.Enum.GetValues<FactionType>())
        { 
			//TODO: Configure starting resources dynamically
			gold[f] = 10000;
			wood[f] = 10000; 
			food[f] = 10000; 
		}
	}
}
