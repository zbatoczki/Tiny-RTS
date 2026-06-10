using Game.Autoload;
using Game.Globals;
using Game.Resources.Unit;
using Godot;
using Godot.Collections;

namespace Game.Buildings;

public partial class Barracks : Building
{
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OnReady();
	}

}
