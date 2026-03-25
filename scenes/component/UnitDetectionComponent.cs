using Game.Groups;
using Game.Units;
using Godot;

namespace Game.Component;

public partial class UnitDetectionComponent : Area2D
{
	[Signal]
	public delegate void UnitDetectedEventHandler(Unit target);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if(body is Unit && body.IsInGroup(GlobalGroups.PLAYER_UNIT))
		{
			EmitSignal(SignalName.UnitDetected, body as Unit);
		}
	}
}
