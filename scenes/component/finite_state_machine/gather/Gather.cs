using Game.Globals;
using Game.Resources.Gathering;
using Game.Units;
using Godot;

namespace Game.FSM;

public partial class Gather : State
{
	[Export] private State IdleState;
	[Export] private State MoveState;
	[Export] private State DeadState;
	[Export] AudioStreamPlayer2D woodChopSfx;
	[Export] AudioStreamPlayer2D goldMineSfx;

	private Worker worker;
	private Timer gatherTimer = new();
	private bool chopSyncConnected;

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
		
		if(worker.GatheringResourceTarget == null || worker.GatheringResourceTarget.IsDepleted) return;

		if(worker.GatheringResourceTarget.Type == ResourceType.Gold)
		{
			worker.Visible = false;
			worker.GatheringResourceTarget.EmitSignal("ResourceGathering");
			goldMineSfx.Play();
		}
		else
		{
			StringName gatheringAnimation = $"gather_{worker.GatheringResourceTarget.Type.ToString().ToLower()}";
			worker.animatedSprite2D.Play(gatheringAnimation);
			if(worker.GatheringResourceTarget.Type == ResourceType.Wood)
			{
				// Play the chop sound at the end of each gather_wood loop so it syncs with the swing.
				worker.animatedSprite2D.AnimationLooped += OnGatherAnimationLooped;
				chopSyncConnected = true;
			}
		}

		gatherTimer.WaitTime = unit.stats.GatherRate;
		gatherTimer.Start();
	}

	public override void Exit()
    {
		worker.Visible = true;
        gatherTimer.Stop();
		woodChopSfx.Stop();
		goldMineSfx.Stop();

		if(chopSyncConnected)
		{
			worker.animatedSprite2D.AnimationLooped -= OnGatherAnimationLooped;
			chopSyncConnected = false;
		}
    }

	// Fires each time gather_wood completes a loop; one chop sound per swing.
	private void OnGatherAnimationLooped()
	{
		if(worker.animatedSprite2D.Animation == "gather_wood")
			woodChopSfx.Play();
	}


	private void GatherResource()
    {
		if (worker.GatheringResourceTarget.IsDepleted)
		{
			worker.GatheringResourceTarget = null;
			return;
		}

		(ResourceType resource, int amount) = worker.GatheringResourceTarget.Gather(10);
		worker.CurrentInventory[resource] += amount;
		worker.PrintCurrentInventory();
		worker.ReturnToCastle();
    }


}
