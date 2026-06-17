using System.Linq;
using Game.Units;
using Godot;

namespace Game.FSM;

public partial class Build : State
{
	[Export]
	private State MoveState;
	[Export]
	private State AttackState;
    [Export]
	private State DeadState;

    public override void Enter()
    {
        GD.Print("Build State entered");
        AnimationName = "build";
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
        base.Enter();
    }

    public override void Exit()
    {
        if(unit is Worker worker)
            worker.CostructionTarget = null;
    }
}
