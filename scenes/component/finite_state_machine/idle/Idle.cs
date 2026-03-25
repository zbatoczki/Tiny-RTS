using Godot;

namespace Game.FSM;

public partial class Idle : State
{
	[Export]
	private State MoveState;
	[Export]
	private State AttackState;
    [Export]
	private State DeadState;

    public override void Enter()
    {
		AnimationName = "idle";
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
        base.Enter();
    }
}
