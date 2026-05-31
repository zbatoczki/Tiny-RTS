using Godot;
using Game.Globals;

namespace Game.Resources;

public abstract partial class ResourceNode : Node2D
{
    [Signal]
    public delegate void ResourceDepletedEventHandler(Vector2I cellCoordinates);

    [Signal]
    public delegate void ResourceGatheredEventHandler(Vector2I cellCoordinates, int amount);

    [Signal]
    public delegate void ResourceGatheringEventHandler();

    [Export] public ResourceType Type;
    [Export] public int          TotalResources   = 1000;
    [Export] public Vector2I Dimensions {get; private set;} = new (1, 1); 

    public Vector2I CellCoordinates {get; set;}
    public bool IsDepleted => RemainingResources <= 0;
    public int  RemainingResources {get; set;}


    protected void InitializeResource(StringName stringName)
    {
        RemainingResources = TotalResources;
        AddToGroup(stringName);
    }


    public (ResourceType, int) Gather(int amount)
    {
        if(IsDepleted) 
        {
            EmitSignal(SignalName.ResourceDepleted, CellCoordinates);
            return (Type, 0);
        }

        int gathered = Mathf.Clamp(Mathf.Min(amount, RemainingResources), 0, RemainingResources);
        RemainingResources -= gathered;

        if (IsDepleted)
        {
            EmitSignal(SignalName.ResourceDepleted, CellCoordinates);
        }

        return (Type, gathered);
    }

}