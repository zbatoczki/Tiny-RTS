using Godot;
using Game.Units;
using Game.Globals;

namespace Game.FSM;

public partial class Attack : State
{
	[Export]
	private State IdleState;
	[Export]
	private State MoveState;
	[Export]
	private State DeadState;

	private Timer attackTimer = new();
    public override void _Ready()
    {
        attackTimer.Timeout += DoAttack;
        AddChild(attackTimer);
    }

	public override void Enter()
	{	
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
		AnimationName = "attack";
		if(unit.HasAttackDirections)
		{
			SetAttackDirection();
		}
		attackTimer.WaitTime /= unit.stats.AttackSpeed;
		unit.animatedSprite2D.AnimationFinished += OnAnimationFinished;
		DoAttack();
	}

    public override void Exit()
    {

		unit.animatedSprite2D.AnimationFinished -= OnAnimationFinished;
        attackTimer.Stop();
    }

	public override State UpdateFrame(double delta)
	{
		if(IsInstanceValid(unit.AttackTarget))
			unit.FaceRight(unit.GlobalPosition - unit.AttackTarget.GlobalPosition > Vector2.Zero);
		
		return null;
	}

	private void OnAnimationFinished()
	{
		unit.PlayAnimation("idle");
		if(!IsInstanceValid(unit.AttackTarget))
		{
			unit.stateMachine.ForceToState<Idle>();
		}
	}


	private void DoAttack()
    {   
		unit.PlayAnimation(AnimationName);
		unit.Attack();
        attackTimer.Start();
    }

	private void SetAttackDirection()
	{
		(float x, float y) = (unit.GlobalPosition - unit.AttackTarget.GlobalPosition).Normalized().Round();
		if(y >= 1)
		{
			AnimationName = AttackAnimationDirections.AtackUp;
			if(x >= 1 || x <= -1)
			{
				AnimationName = AttackAnimationDirections.UpRight;
				if(x < 0) unit.FaceRight(false);
			}
		}
		else if(y <= -1)
		{
			AnimationName = AttackAnimationDirections.AtackDown;
			if(x >= 1 || x <= -1)
			{
				AnimationName = AttackAnimationDirections.DownRight;
				if(x < 0) unit.FaceRight(false);
			}
		}
	}
	

}
