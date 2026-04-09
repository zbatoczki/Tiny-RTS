using Game.Units;
using Godot;

namespace Game.FSM;

public partial class Gather : State
{
	[Export]
	private State IdleState;
	[Export]
	private State MoveState;
	[Export]
	private State DeadState;

	private Worker worker;
	private Timer gatherTimer = new();

	public override void _Ready()
	{
		gatherTimer.Timeout += GatherResource;
		AddChild(gatherTimer);
	}

    public override State UpdateFrame(double delta)
    {
        if(worker.GatheringResourceTarget == null) return IdleState;

		return null;
    }


    public override void Enter()
	{
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;
		worker = unit as Worker;
		
		if(worker.GatheringResourceTarget == null) return;

		if(worker.GatheringResourceTarget.Name == "gold")
		{
			worker.Visible = false;
			worker.GatheringResourceTarget.EmitSignal("ResourceGathering");
		}
		else
		{
			StringName gatheringAnimation = $"gather_{worker.GatheringResourceTarget.Name}";
			worker.animatedSprite2D.Play(gatheringAnimation);
		}

		worker.GatheringResourceTarget.IsBeingGathered = true;
		gatherTimer.WaitTime = unit.stats.GatherRate;
		gatherTimer.Start();
	}

	public override void Exit()
    {
		worker?.GatheringResourceTarget?.IsBeingGathered = false;
		worker.Visible = true;
        gatherTimer.Stop();
    }


	private void GatherResource()
    {
		if (worker.GatheringResourceTarget.IsDepleted)
		{
			worker.GatheringResourceTarget = null;
			return;
		}

		(string resource, int amount) = worker.GatheringResourceTarget.Gather(10);
		worker.CurrentInventory[resource] += amount;
		GD.Print("Current worker Inventory");
		worker.PrintCurrentInventory();
		worker.ReturnToCastle();
    }


}
