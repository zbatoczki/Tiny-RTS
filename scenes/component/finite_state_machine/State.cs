using Game.Units;
using Godot;


namespace Game.FSM;

[GlobalClass]
public abstract partial class State : Node
{
	public Unit unit;

	protected StringName AnimationName;

	public virtual void Enter()
	{
		unit.PlayAnimation(AnimationName);
	}

	public virtual void Exit()
	{
		return;
	}

	public virtual State ProcessInput(InputEvent inputEvent)
	{
		return null;
	}

	public virtual State UpdateFrame(double delta)
	{
		return null;
	}

	public virtual State UpdatePhysicsFrame(double delta)
	{
		return null;
	}
}
