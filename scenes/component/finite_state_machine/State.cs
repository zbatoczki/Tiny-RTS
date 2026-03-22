using Godot;
using System;

namespace Game.FSM;

[GlobalClass]
public abstract partial class State : Node
{
	[Signal]
	public delegate void StateFinishedEventHandler(State targetState);

	[Export]
	public AnimatedSprite2D animatedSprite2D;
	[Export]
	public StringName AnimationName;

	public virtual void Enter()
	{
		animatedSprite2D.Play(AnimationName);
	}

	public virtual void Exit()
	{
		return;
	}

	public virtual State ProcessInput(InputEvent inputEvent)
	{
		return null;
	}

	public virtual State UpdateFrame(float delta)
	{
		return null;
	}

	public virtual State UpdatePhysicsFrame(float delta)
	{
		return null;
	}
}
