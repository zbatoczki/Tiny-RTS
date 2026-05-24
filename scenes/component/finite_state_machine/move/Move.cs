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
        Vector2 goToPosition;
        bool atFinalWaypoint;

        if (unit.AttackTarget != null)
        {
            // Chasing a moving target — A* path would be stale immediately, use direct steering.
            goToPosition = unit.AttackTarget.CenterPosition;
            atFinalWaypoint = true;
        }
        else if (unit.path.Length > 0)
        {
            while (unit.pathIndex < unit.path.Length - 1 &&
                   unit.CenterPosition.DistanceTo(unit.path[unit.pathIndex]) <= 5f)
            {
                unit.pathIndex++;
            }
            goToPosition = unit.path[unit.pathIndex];
            atFinalWaypoint = unit.pathIndex == unit.path.Length - 1;
        }
        else
        {
            goToPosition = unit.targetPosition;
            atFinalWaypoint = true;
        }

		var direction = goToPosition - unit.CenterPosition;

        unit.FaceRight(direction.X < 0);

        if(atFinalWaypoint && direction.Length() <= 5f)
        {
            unit.path = [];
            unit.QueueRedraw();
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
				AnimationName += $"_{mostCarried.Key.ToString().ToLower()}";
		}
	}

}
