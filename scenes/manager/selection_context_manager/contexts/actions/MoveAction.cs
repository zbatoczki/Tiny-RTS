using System.Collections.Generic;
using Game.Selection;
using Game.Units;
using Godot;

namespace Game.Context.Actions;

/// <summary>
/// Fallback action — moves all selected units to the click position in a grid formation.
/// Always returns true from CanHandle so it acts as the catch-all.
/// </summary>
public sealed class MoveAction : IContextAction
{
    private const float Spacing = 64f;

    // Lowest priority — runs only when no other action matched.
    public int Priority => 0;

    public bool CanHandle(SelectionContext context, GodotObject target)
        => context.HasUnits;

    public void Execute(SelectionContext context, GodotObject target, Vector2 mousePosition)
    {
        // Clear targets so units stop chasing.
        foreach (Unit unit in context.Units)
        {
            unit.AttackTarget = null;
            if(unit is Worker worker)
                worker.ConstructionTarget = null;
        }

        List<Vector2> positions = GetFormationPositions(mousePosition, context.Units.Count);
        for (int i = 0; i < context.Units.Count; i++)
            context.Units[i].MoveTo(positions[i]);
    }

    private static List<Vector2> GetFormationPositions(Vector2 center, int unitCount)
    {
        var positions = new List<Vector2>(unitCount);

        int cols = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
        int rows = Mathf.CeilToInt((float)unitCount / cols);

        float gridWidth  = (cols - 1) * Spacing;
        float gridHeight = (rows - 1) * Spacing;
        Vector2 origin = center - new Vector2(gridWidth / 2f, gridHeight / 2f);

        for (int i = 0; i < unitCount; i++)
        {
            int col = i % cols;
            int row = i / cols;
            positions.Add(origin + new Vector2(col * Spacing, row * Spacing));
        }

        return positions;
    }
}