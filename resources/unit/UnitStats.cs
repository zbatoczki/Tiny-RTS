using Godot;

namespace Game.Resources.Unit;

[GlobalClass]
public partial class UnitStats : Resource
{
    [Export] public string Name { get; set; }
    [Export] public float MaxHealth { get; set; } = 10f;
    [Export] public float AttackSpeed { get; set; } = 1f; //in seconds for cooldown
    [Export] public float AttackDamage { get; set; } = 1f;
    [Export] public float MovementSpeed { get; set; } = 150f;
    [Export] public float AttackRange { get; set; }
    [Export] public float VisionRange { get; set; }

}