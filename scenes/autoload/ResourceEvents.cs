using Godot;

namespace Game.Autoload;

public partial class ResourceEvents : Node
{
	public static ResourceEvents Instance {get; private set;}

	[Signal]
	public delegate void ResourcesModifiedEventHandler(int woodAmt = 0, int goldAmt = 0, int foodAmt = 0);

	public override void _Notification(int what)
    {
        if(what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static void EmitResourcesModified(int woodAmt = 0, int goldAmt = 0, int foodAmt = 0)
	{
		Instance.EmitSignal(SignalName.ResourcesModified, woodAmt, goldAmt, foodAmt);
	}
	
}
