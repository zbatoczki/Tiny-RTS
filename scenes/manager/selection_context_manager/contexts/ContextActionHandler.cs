using System.Collections.Generic;
using System.Linq;
using Game.Context.Actions;
using Game.Resources;
using Game.Selection;
using Godot;
using Godot.Collections;

namespace Game.Context;

/// <summary>
/// Resolves and executes the correct <see cref="IContextAction"/> for a right-click.
///
/// Usage:
///   1. Create an instance, passing in any dependencies actions need.
///   2. Call HandleRightClick() from SelectionManager on right-click.
///   3. Register additional custom actions with RegisterAction().
/// </summary>
public sealed class ContextActionHandler(Node2D _sceneRoot, TreeTileMapLayerManager treeTileMapLayer)
{
    private readonly List<IContextAction> actions =
        [
            new AttackAction(),
            new GatherTreeAction(treeTileMapLayer),
            new GatherGoldAction(),
            new MoveAction(), // catch-all — must remain last
        ];

    private readonly Node2D sceneRoot = _sceneRoot; // used for physics queries

    /// <summary>Adds a custom action. The list is re-sorted by priority automatically.</summary>
    public void RegisterAction(IContextAction action)
    {
        actions.Add(action);
        actions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// Queries physics space at <paramref name="mousePosition"/>, finds the
    /// first matching action for the current context, and executes it.
    /// </summary>
    public void HandleRightClick(SelectionContext context, Vector2 mousePosition)
    {
        if (context.Type == SelectionContext.SelectionType.None) return;

        GodotObject target = GetTopColliderAt(mousePosition);

        IContextAction action = actions.FirstOrDefault(a => a.CanHandle(context, target));
        action?.Execute(context, target, mousePosition);
    }

    private GodotObject GetTopColliderAt(Vector2 worldPosition)
    {
        PhysicsDirectSpaceState2D spaceState = sceneRoot.GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D { Position = worldPosition };
        Array<Dictionary> hits = spaceState.IntersectPoint(query);

        if (hits.Count == 0) return null;

        foreach (Dictionary hit in hits)
        {
            if (hit.TryGetValue("collider", out Variant colliderVariant))
                return (GodotObject)colliderVariant.Obj;
        }

        return null;
    }
}