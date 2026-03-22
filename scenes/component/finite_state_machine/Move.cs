using Godot;
using System;

namespace Game.FSM;

public partial class Move : State
{

	[Export]
	private State idleState;

	public override void Enter()
    {
        base.Enter();
    }
}
