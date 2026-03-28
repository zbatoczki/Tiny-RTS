using Godot;

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
		if (IsInstanceValid(unit.AttackTarget))
		{
			unit.AttackTarget.TakeDamage(unit.stats.AttackDamage);
		}
        attackTimer.Start();
    }
	

}
