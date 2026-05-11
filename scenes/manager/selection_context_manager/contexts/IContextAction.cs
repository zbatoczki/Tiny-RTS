using Game.Selection;
using Godot;

namespace Game.Context;

/// <summary>
/// A single context-sensitive action that fires on right-click.
/// Actions are evaluated in descending Priority order; the first one
/// whose CanHandle() returns true is executed.
/// </summary>
public interface IContextAction
{
    /// <summary>
    /// Higher value = evaluated first. Use this to ensure specific
    /// actions (attack, gather) take precedence over the generic move.
    /// </summary>
    int Priority { get; }
 
    /// <summary>
    /// Return true if this action applies to the current selection + click target.
    /// <paramref name="target"/> is the top-most physics body under the cursor,
    /// or null if the player clicked empty space.
    /// </summary>
    bool CanHandle(SelectionContext context, GodotObject target);
 
    /// <summary>
    /// Execute the action.
    /// </summary>
    void Execute(SelectionContext context, GodotObject target, Vector2 mousePosition);

}