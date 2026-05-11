using Game.Buildings;
using Game.Selection;
using Game.Units;
using Godot;

namespace Game.Context.Actions;

/// <summary>
/// Orders selected worker units to gather from a right-clicked gold mine.
/// </summary>
public sealed class GatherGoldAction : IContextAction
{
    public int Priority => 90;

    public bool CanHandle(SelectionContext context, GodotObject target)
        => context.HasWorkers && target is GoldMine mine && !mine.IsDepleted;

    public void Execute(SelectionContext context, GodotObject target, Vector2 mousePosition)
    {
        if (target is not GoldMine goldMine) return;

        foreach (Worker worker in context.Workers)
        {
            worker.GatheringResourceTarget = goldMine;
        }
    }
}