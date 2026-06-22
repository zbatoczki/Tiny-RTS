using System.Linq;
using Game.Globals;
using Game.Units;
using Godot;

namespace Game.FSM;

public partial class Build : State
{
	[Export] private State MoveState;
	[Export] private State AttackState;
    [Export] private State DeadState;
    
    private Timer buildTimer = new();

    public override void _Ready()
    {
        buildTimer.WaitTime = 3;
        buildTimer.Timeout += OnBuildTimerFinished;
        AddChild(buildTimer);
    }
    
    public override void Enter()
    {
        AnimationName = "build";
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
        base.Enter();
        
        buildTimer.Start();
    }

    public override void Exit()
    {
        buildTimer.Stop();
        Worker worker = unit as Worker;
        if(IsInstanceValid(worker.ConstructionTarget))
            worker.ConstructionTarget.BuildingConstructed -= worker.OnBuldingConstructed;
    }

    public void OnBuildTimerFinished()
    {
        Worker worker = unit as Worker;
        worker.Construct();
        buildTimer.Start();
    }
}
