using Game.FSM;
using Game.Units;
using Godot;
using System;

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

    public override void Enter()
	{
		unit.targetPosition = Vector2.Zero;
		unit.Velocity = Vector2.Zero;

		worker = unit as Worker;
		StringName gatheringAnimation = $"gather_{worker.GatheringResourceTarget.Name.ToLower()}";
		worker.animatedSprite2D.Play(gatheringAnimation);

		gatherTimer.WaitTime = unit.stats.GatherRate;

		GatherResource();
	}

	public override void Exit()
    {
        gatherTimer.Stop();
    }


	private void GatherResource()
    {
		(string resource, int amount) = worker.GatheringResourceTarget.Gather(1);
		worker.CurrentInventory[resource] += amount;
		gatherTimer.Start();
    }


}
