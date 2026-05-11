using Game.Groups;
using Game.Selection;
using Game.Units;
using Godot;

namespace Game.Context.Actions;

/// <summary>
/// Orders all selected units to attack a right-clicked enemy.
/// </summary>
public sealed class AttackAction : IContextAction
{
    public int Priority => 100;

    public bool CanHandle(SelectionContext context, GodotObject target)
        => context.HasUnits
           && target is Unit enemy
           && enemy.IsInGroup(GlobalGroups.ENEMY_UNIT);

    public void Execute(SelectionContext context, GodotObject target, Vector2 mousePosition)
    {
        if (target is not Unit enemy) return;
        
        foreach(Unit unit in context.Units)
        {
            unit.AttackTarget = enemy;
        }
    }
}