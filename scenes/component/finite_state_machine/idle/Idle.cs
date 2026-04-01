using System.Linq;
using Game.Units;
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
		SetAnimation();
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
        base.Enter();
    }

	private void SetAnimation()
	{
		AnimationName = "idle";
		if(unit is Worker worker)
		{
			var mostCarried = worker.CurrentInventory.MaxBy(entry => entry.Value);
			if(mostCarried.Value > 0)
				AnimationName += $"_{mostCarried.Key}";
		}
	}
}
