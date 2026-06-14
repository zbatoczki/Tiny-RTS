using System.Collections.Generic;
using Godot;

namespace Game;

/// <summary>Extension methods for working with integer rectangles as tile areas.</summary>
public static class Rect2IExtensions
{
    /// <summary>
    /// Expands the rectangle into the list of Vector2I cell coordinates it
    /// covers (its <c>Position</c> inclusive, its <c>End</c> exclusive).
    /// </summary>
    public static List<Vector2I> ToCells(this Rect2I rect)
    {
        List<Vector2I> cells = [];
		for(int x = rect.Position.X; x < rect.End.X; x++)
		{
			for(int y = rect.Position.Y; y < rect.End.Y; y++)
			{
				cells.Add(new Vector2I(x, y));
			}
		}
        return cells;
    }

	/// <summary>Returns a floating-point <see cref="Rect2"/> copy of this rectangle.</summary>
	public static Rect2 ToRect2F(this Rect2I rect)
	{
		return new Rect2(rect.Position, rect.Size);
	}
}
