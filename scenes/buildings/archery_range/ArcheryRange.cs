using Game.Autoload;
using Game.Globals;
using Game.Resources.Unit;
using Godot;
using Godot.Collections;

namespace Game.Buildings;

public partial class ArcheryRange : Building
{
	public override void _Ready()
	{
		OnReady();
	}
}
