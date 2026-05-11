using System.Collections.Generic;
using Godot;

namespace Game.Context;

/// <summary>
/// Computes grid formation positions centered on a target point.
/// Used by both <see cref="Actions.MoveAction"/> and gather actions so
/// workers physically walk to the resource after being assigned a target.
/// </summary>
public static class FormationHelper
{
    private const float SPACING = 64f;

    /// <summary>
    /// Returns one world-space position per unit, arranged in a grid
    /// centered on <paramref name="center"/>.
    /// </summary>
    public static List<Vector2> GetPositions(Vector2 center, int unitCount)
    {
        var positions = new List<Vector2>(unitCount);

        if (unitCount == 0) return positions;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
        int rows = Mathf.CeilToInt((float)unitCount / cols);

        float gridWidth  = (cols - 1) * SPACING;
        float gridHeight = (rows - 1) * SPACING;
        Vector2 origin   = center - new Vector2(gridWidth / 2f, gridHeight / 2f);

        for (int i = 0; i < unitCount; i++)
        {
            int col = i % cols;
            int row = i / cols;
            positions.Add(origin + new Vector2(col * SPACING, row * SPACING));
        }

        return positions;
    }

    /// <summary>
    /// Convenience overload: moves each unit in <paramref name="units"/> to
    /// its assigned formation slot around <paramref name="center"/>.
    /// </summary>
    public static void MoveToFormation<T>(IReadOnlyList<T> units, Vector2 center)
        where T : Units.Unit
    {
        List<Vector2> positions = GetPositions(center, units.Count);
        for (int i = 0; i < units.Count; i++)
            units[i].MoveTo(positions[i]);
    }
}