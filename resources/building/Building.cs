using Game.Globals;
using Godot;

namespace Game.Resources.Building;

public partial class Building : StaticBody2D
{
	[Export] public Faction Faction;
}
