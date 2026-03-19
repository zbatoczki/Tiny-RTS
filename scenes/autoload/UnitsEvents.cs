using Godot;

namespace Game.Autoload;

public partial class UnitsEvents : Node
{
    public static UnitsEvents Instance {get; private set;}


    // Unit announces it wants to move — GridManager will validate
    [Signal]
    public delegate void UnitMoveRequestedEventHandler(Vector2 targetWorldPos);

    [Signal]
	public delegate void UnitMovementFinsishedEventHandler(Vector2 globalPosition);
	[Signal]
	public delegate void UnitMovementStartedEventHandler(Vector2 globalPosition);

    public override void _Notification(int what)
    {
        if(what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
    }

    public static void EmitUnitMovementRequested(Vector2 targetCellPosition)
    {
        Instance.EmitSignal(SignalName.UnitMoveRequested, targetCellPosition);
    }

    public static void EmitUnitMovementFinished(Vector2 globalPosition)
	{
		Instance.EmitSignal(SignalName.UnitMovementFinsished, globalPosition);
	}

	public static void EmitUnitMovementStarted(Vector2 globalPosition)
	{
		Instance.EmitSignal(SignalName.UnitMovementStarted, globalPosition);
	}
    
}