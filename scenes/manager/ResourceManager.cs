using System.Collections.Generic;
using Game.Globals;
using Godot;

namespace Game.Autoload;

public partial class ResourceManager : Node
{
	public static ResourceManager Instance { get; private set; }

	[Signal] public delegate void ResourceChangedEventHandler(int faction, int gold, int wood, int food);

	private readonly Dictionary<Faction, int> gold = [];
    private readonly Dictionary<Faction, int> wood = [];
    private readonly Dictionary<Faction, int> food = [];

	private int CurrentWoodAmount = 0;
	private int CurrentGoldAmount = 0;
	private int CurrentFoodAmount = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//ResourceEvents.Instance.ResourcesModified += OnResourcesModified;

		if (Instance != null) 
		{ 
			QueueFree(); 
			return;
		}
        Instance = this;
        foreach (Faction f in System.Enum.GetValues<Faction>())
        { 
			gold[f] = 200;
			wood[f] = 200; 
			food[f] = 200; 
		}
	}

	public int  GetGold(Faction f) => gold[f];
    public int  GetWood(Faction f) => wood[f];
	public int  GetFood(Faction f) => food[f];

	public bool CanAfford(Faction faction, int woodCost = 0, int goldCost = 0, int foodCost = 0)
	{
		return 
		gold[faction] >= goldCost && 
		wood[faction] >= woodCost &&
		food[faction] >= foodCost;
	}

	public bool Spend(Faction faction, int woodCost = 0, int goldCost = 0, int foodCost = 0)
	{
		if(!CanAfford(faction, woodCost, goldCost, foodCost))
		{
			return false;
		}

		gold[faction] -= goldCost;
		wood[faction] -= woodCost;
		food[faction] -= foodCost;

		EmitSignal(SignalName.ResourceChanged, (int) faction, gold[faction], wood[faction], food[faction]);

		return true;
	}

	public void AddResource(Faction faction, ResourceType type, int amount)
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
}
