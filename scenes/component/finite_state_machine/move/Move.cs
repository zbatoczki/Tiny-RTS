using System.Linq;
using Game.Units;
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
        SetAnimation();
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

    private void SetAnimation()
	{
		AnimationName = "move";
		if(unit is Worker worker)
		{
			var mostCarried = worker.CurrentInventory.MaxBy(entry => entry.Value);
			if(mostCarried.Value > 0)
				AnimationName += $"_{mostCarried.Key}";
		}
	}

}
