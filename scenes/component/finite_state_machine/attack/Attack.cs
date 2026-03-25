using Game.FSM;
using Godot;

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
		unit.PlayAnimation(AnimationName);
	}

    public override void Exit()
    {
		unit.animatedSprite2D.AnimationFinished -= OnAnimationFinished;
        attackTimer.Stop();
    }

	public override State UpdateFrame(double delta)
	{
		if(IsInstanceValid(unit.attackTarget))
			unit.FaceRight(unit.GlobalPosition - unit.attackTarget.GlobalPosition > Vector2.Zero);
		
		return null;
	}

	private void OnAnimationFinished()
	{
		GD.Print("On animation finished");
		if (unit.animatedSprite2D.Animation != AnimationName) return; // ignore other animations finishing

		if (IsInstanceValid(unit.attackTarget))
		{
			unit.attackTarget.TakeDamage(unit.stats.AttackDamage);

			// still has a target — attack again or go idle based on your design
			unit.PlayAnimation(AnimationName); // loop manually if in range
			// OR: EmitSignal(nameof(StateFinished), IdleState);
		}
		// else
		// {
		// 	EmitSignal(nameof(StateFinished), IdleState);
		// }
	}


	private void DoAttack()
    {   
		GD.Print("DoAttack Start");
		
        attackTimer.Start();
        //send signal to damage target
        
    }
	

}
