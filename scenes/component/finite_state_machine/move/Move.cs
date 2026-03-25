using Godot;

namespace Game.FSM;

public partial class Move : State
{

	[Export]
	private State IdleState;
    [Export]
	private State DeadState;
    [Export]
	private State AttackState;

	public override void Enter()
    {
        AnimationName = "move";
        unit.damageComponent.Monitoring = unit.AttackTarget != null;
        base.Enter();
    }

    public override State UpdatePhysicsFrame(double delta)
	{
        var goToPosition = unit.AttackTarget != null ? unit.AttackTarget.GlobalPosition : unit.targetPosition;
        
		var direction = goToPosition - unit.GlobalPosition;

        unit.FaceRight(direction.X < 0);

        if(direction.Length() <= 5f)
        {
            return IdleState;
        }

        unit.Velocity = direction.Normalized() * unit.stats.MovementSpeed;
        unit.MoveAndSlide();

        return null;
	}
}
