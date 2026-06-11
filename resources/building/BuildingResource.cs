using Game.Globals;
using Godot;
using Godot.Collections;

namespace Game.Resources.Building;

[GlobalClass]
public partial class BuildingResource : Resource
{
	[Export] public string Name {get; private set;}
	[Export] public string Description {get; private set;}
	[Export] public Vector2I Dimensions {get; private set;}
	[Export] public int BuildableRadius {get; private set;}
	[Export] public int ResourceRadius {get; private set;}
	[Export] public int AttackRadius {get; private set;}
	[Export] public PackedScene BuildingScene {get; private set;}
	[Export] public PackedScene SpriteScene {get; private set;}
	[Export] public bool IsResourceDropOff {get; private set;}
	[Export] public Dictionary<ResourceType, int> ResourceCosts { get; set; }
}
