using Godot;

namespace Game.Resources.Gathering;

[GlobalClass]
public partial class GatheringResource : Resource
{
    [Signal]
    public delegate void ResourceDepletedEventHandler(Vector2I cellCoordinates);

    [Signal]
    public delegate void ResourceGatheredEventHandler(Vector2I cellCoordinates, int amount);

    [Signal]
    public delegate void ResourceGatheringEventHandler();

    public enum ResourceTypes
    {
        WOOD,
        FOOD,
        GOLD
    }

    [Export] public Vector2I CellCorrdinates {get; set;}
    [Export] public ResourceTypes ResourceType {get; set;}
    [Export] public int MaxCharges {get; set;} = 100;
    [Export] public int CurrentCharges {get; set;} = 100;
    [Export] public bool IsBeingGathered {get; set;} = false;

    public bool IsDepleted => CurrentCharges <= 0;

    /// <summary>
    /// A worker unit will call this method to subtract the amount they want to gether.
    /// </summary>
    /// <param name="amount">Amount to subtract from the current charges</param>
    /// <returns></returns>
    public (ResourceTypes, int) Gather(int amount)
    {
        EmitSignal(SignalName.ResourceGathering);
        if(IsDepleted) 
        {
            EmitSignal(SignalName.ResourceDepleted, CellCorrdinates);
            return (ResourceType, 0);
        }

        int gathered = Mathf.Clamp(Mathf.Min(amount, CurrentCharges), 0, CurrentCharges);
        CurrentCharges -= gathered;
        GD.Print($"Current charges for resource at {CellCorrdinates}: {CurrentCharges}");
        EmitSignal(SignalName.ResourceGathered, CellCorrdinates, gathered);

        if (IsDepleted)
        {
            EmitSignal(SignalName.ResourceDepleted, CellCorrdinates);
        }

        return (ResourceType, gathered);
    }
}