using Godot;
using System;

namespace Game.FSM;

public partial class Idle : State
{
	[Export]
	private State moveState;


    public override void Enter()
    {
        base.Enter();
    }

	public override State ProcessInput(InputEvent inputEvent)
	{
		return null;
	}
	
}
