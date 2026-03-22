using Godot;
using System.Linq;

namespace Game.FSM;

public partial class StateMachine : Node
{
	[Export]
	private State InitialState = null;

	private State currentState = null;

    public override void _Ready()
    {
        currentState = InitialState ?? GetChild(0) as State;

		foreach(var state in GetChildren().Cast<State>())
		{
			state.StateFinished += ChangeToState;
		}

		Owner.Ready += () => currentState.Enter();
    }

	private void ChangeToState(State newState)
	{
		currentState?.Exit();
		currentState = newState;
		currentState.Enter();
	}

	public void UpdateFrame(float delta)
	{
		State newState = currentState.UpdateFrame(delta);
		if(newState != null) ChangeToState(newState);
	}

	public void UpdatePhysicsFrame(float delta)
	{
		State newState = currentState.UpdatePhysicsFrame(delta);
		if(newState != null) ChangeToState(newState);
	}

	public void ProcessInput(InputEvent inputEvent)
	{
		State newState = currentState.ProcessInput(inputEvent);
		if(newState != null) ChangeToState(newState);
	}

}
