using Game.Units;
using Godot;
using System.Linq;

namespace Game.FSM;

public partial class StateMachine : Node
{
	[Export]
	private State StartingState = null;

	public State currentState {get; private set;} = null;

	public void Init(Unit parent)
	{
		foreach(State state in GetChildren().Cast<State>())
		{
			state.unit = parent;
		}
		ChangeToState(StartingState);
	}

	public void UpdateFrame(double delta)
	{
		State newState = currentState.UpdateFrame(delta);
		if(newState != null) ChangeToState(newState);
	}

	public void UpdatePhysicsFrame(double delta)
	{
		State newState = currentState.UpdatePhysicsFrame(delta);
		if(newState != null) ChangeToState(newState);
	}

	public void ProcessInput(InputEvent inputEvent)
	{
		State newState = currentState.ProcessInput(inputEvent);
		if(newState != null) ChangeToState(newState);
	}

	public void ForceToState<T>() where T : State
	{
		State targetState = GetChildren().OfType<T>().FirstOrDefault();
		if(targetState != null)
		{
			ChangeToState(targetState);
		}
		else
		{
			GD.PushWarning($"StateMachine: No state of type {typeof(T).Name} found.");
		}
	}

	private void ChangeToState(State newState)
	{
		currentState?.Exit();
		currentState = newState;
		currentState.Enter();
	}

}
