using Godot;

namespace Game.Resources.Building;

[GlobalClass]
public partial class BuildingResource : Resource
{
	[Export] public string Name {get; private set;}
	[Export] public string Description {get; private set;}
	[Export] public int BuildableRadius {get; private set;}
	[Export] public int ResourceRadius {get; private set;}
	[Export] public int AttackRadius {get; private set;}
	[Export] public PackedScene BuildingScene {get; private set;}
	[Export] public bool IsResourceDropOff {get; private set;}
}
