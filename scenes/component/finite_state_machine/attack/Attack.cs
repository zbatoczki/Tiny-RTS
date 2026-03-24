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

	private Timer attackTimer;
    public override void _Ready()
    {
        attackTimer = new();
        attackTimer.Timeout += DoAttack;
        AddChild(attackTimer);
    }

	public override void Enter()
	{	
		GD.Print("Attack State entered");
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
		AnimationName = "attack";
		DoAttack();
	}

    public override void Exit()
    {
        attackTimer.Stop();
    }

	public override State UpdateFrame(double delta)
	{
		if(IsInstanceValid(unit.attackTarget))
			unit.FaceRight(unit.GlobalPosition - unit.attackTarget.GlobalPosition > Vector2.Zero);
		
		return null;
	}


	private void DoAttack()
    {   
        attackTimer.Start();
		unit.PlayAnimation(AnimationName);
        //send signal to damage target
        
    }
	

}
