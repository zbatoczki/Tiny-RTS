using Game.Globals;
using Godot;
using Godot.Collections;

namespace Game.Resources.Unit;

[GlobalClass]
public partial class UnitResource : Resource
{
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public float MaxHealth { get; set; } = 10f;
    [Export] public float AttackSpeed { get; set; } = 1f; //in seconds for cooldown
    [Export] public float AttackDamage { get; set; } = 1f;
    [Export] public float MovementSpeed { get; set; } = 150f;
    [Export] public float AttackRange { get; set; } = 1f;
    [Export] public float VisionRange { get; set; } = 1f;
    [Export] public float GatherRate {get; set; } = 1f;
    [Export] public float RepairRate {get; set;} = 0f;
    [Export] public FactionType Faction {get; set;}
    [Export] public PackedScene UnitScene { get; set; }
    [Export(PropertyHint.File, "*.tres")] public string IconPath { get; set; }
    [Export] public Dictionary<ResourceType, int> ResourceCosts { get; set; }
}