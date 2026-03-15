using Game.Units;
using Godot;
using System.Collections.Generic;

namespace Game.Component;

public partial class SelectionManager : Control
{
	[Export] public Color SelectionFillColor = new(0.2f, 0.6f, 1.0f, 0.15f);
    [Export] public Color SelectionBorderColor = new(0.2f, 0.6f, 1.0f, 0.85f);
    [Export] public float BorderWidth = 1.5f;

	[Export] public float DragThreshold = 6f;

    [Export] public TileMapLayer TileMapLayer;
    private const float TILE_SIZE = 64f;

	private bool isDragging = false;
    private bool mouseDown = false;
    private Vector2 dragStart = Vector2.Zero;
    private Vector2 dragCurrent = Vector2.Zero;

	private readonly List<Unit> selectedUnits = [];

	public override void _Ready()
    {
        // Make sure mouse events reach this Control even when nothing is
        // visually "underneath" the cursor.
        MouseFilter = MouseFilterEnum.Ignore;

        if (TileMapLayer == null)
            GD.PushWarning("SelectionManager: TileMapLayer export is not assigned. Right-click movement will use raw world position.");
    }

	public override void _Input(InputEvent evt)
    {
        if (evt is InputEventMouseButton mouseButton)
            HandleMouseButton(mouseButton);

        if (evt is InputEventMouseMotion mouseMotion)
            HandleMouseMotion(mouseMotion);
    }

	public override void _Draw()
    {
        if (!isDragging) return;

        Rect2 rect = GetSelectionRect();
        DrawRect(rect, SelectionFillColor);
        DrawRect(rect, SelectionBorderColor, filled: false, width: BorderWidth);
    }

	private void HandleMouseButton(InputEventMouseButton e)
    {
        if (e.ButtonIndex == MouseButton.Right && e.Pressed)
        {
            if (selectedUnits.Count > 0)
            {
                IssueMovementOrder(e.Position);
            }
            return;
        }

        if (e.ButtonIndex != MouseButton.Left) return;

        if (e.Pressed)
        {
            mouseDown   = true;
            isDragging  = false;
            dragStart   = e.Position;
            dragCurrent = e.Position;
        }
        else // released
        {
            if (isDragging)
                FinishBoxSelect();
            else
                FinishClickSelect(e.Position);

            mouseDown  = false;
            isDragging = false;
            QueueRedraw();
        }
    }

	private void HandleMouseMotion(InputEventMouseMotion e)
    {
        if (!mouseDown) return;

        dragCurrent = e.Position;

        if (!isDragging && dragStart.DistanceTo(dragCurrent) >= DragThreshold)
            isDragging = true;

        if (isDragging)
            QueueRedraw();
    }

    /// Converts the right-click screen position to the nearest tile centre,
    /// then dispatches staggered formation targets to each selected unit.
    private void IssueMovementOrder(Vector2 screenPos)
    {
        //Vector2 worldPos   = ScreenToWorld(screenPos);
        //Vector2 tileCenter = SnapToTileCenter(worldPos);

        Vector2 mousePosition = GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		Vector2 tileCenter = gridPosition.Floor() * 64;

        List<Vector2> offsets = GetFormationOffsets(selectedUnits.Count);

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (IsInstanceValid(selectedUnits[i]))
            {
                var finalPosition = tileCenter + offsets[i];
                selectedUnits[i].MoveTo(finalPosition);
            }
        }
    }

    /// Snaps a world-space position to the centre of the nearest tile.
    /// Falls back to the raw world position if no TileMapLayer is assigned.
    private Vector2 SnapToTileCenter(Vector2 worldPos)
    {
        if (TileMapLayer == null)
            return worldPos;

        // Convert to TileMapLayer local space, get tile coords, convert back.
        Vector2  localPos   = TileMapLayer.ToLocal(worldPos);
        Vector2I tileCoords = TileMapLayer.LocalToMap(localPos);
        Vector2  tileLocal  = TileMapLayer.MapToLocal(tileCoords);
        return TileMapLayer.ToGlobal(tileLocal);
    }

    /// Returns a list of world-space offsets for a formation of <paramref name="unitCount"/>
    /// units, radiating outward from the centre in tile-sized steps.
    ///
    /// Ring 0 → centre tile (0, 0)
    /// Ring 1 → the 8 adjacent tiles
    /// Ring 2 → the next 16 tiles, and so on.
    private List<Vector2> GetFormationOffsets(int unitCount)
    {
        var offsets = new List<Vector2>(unitCount);
        int ring    = 0;

        while (offsets.Count < unitCount)
        {
            if (ring == 0)
            {
                offsets.Add(Vector2.Zero);
            }
            else
            {
                // Walk the perimeter of the square ring clockwise.
                for (int x = -ring; x <= ring && offsets.Count < unitCount; x++)
                    offsets.Add(new Vector2(x, -ring) * TILE_SIZE);

                for (int y = -ring + 1; y <= ring && offsets.Count < unitCount; y++)
                    offsets.Add(new Vector2(ring, y) * TILE_SIZE);

                for (int x = ring - 1; x >= -ring && offsets.Count < unitCount; x--)
                    offsets.Add(new Vector2(x, ring) * TILE_SIZE);

                for (int y = ring - 1; y > -ring && offsets.Count < unitCount; y--)
                    offsets.Add(new Vector2(-ring, y) * TILE_SIZE);
            }

            ring++;
        }

        return offsets;
    }


	private void FinishClickSelect(Vector2 screenPos)
    {
        // Convert screen position to world position so we can compare with
        // unit positions (which live in world space).
        Vector2 worldPos = ScreenToWorld(screenPos);

        DeselectAll();

        // Find the closest unit whose collision area contains the click point.
        // We ask each unit whether the click hits it; this keeps the Unit
        // responsible for its own hit-detection shape.
        Unit closest  = null;
        float minDist = float.MaxValue;

        foreach (Unit unit in GetAllUnits())
        {
            if (!unit.ContainsPoint(worldPos)) continue;

            float dist = unit.GlobalPosition.DistanceTo(worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = unit;
            }
        }

        if (closest != null)
            SelectUnit(closest);
    }


    private void FinishBoxSelect()
    {
        Rect2 screenRect = GetSelectionRect();
        DeselectAll();

        Camera2D cam = GetViewport().GetCamera2D();

        foreach (Unit unit in GetAllUnits())
        {
            Vector2 screenPos = cam != null
                ? WorldToScreen(unit.GlobalPosition)
                : unit.GlobalPosition;

            if (screenRect.HasPoint(screenPos))
                SelectUnit(unit);
        }
    }

	private Rect2 GetSelectionRect()
    {
        Vector2 topLeft = new Vector2(
            Mathf.Min(dragStart.X, dragCurrent.X),
            Mathf.Min(dragStart.Y, dragCurrent.Y));
        Vector2 size = (dragCurrent - dragStart).Abs();
        return new Rect2(topLeft, size);
    }

	private void SelectUnit(Unit unit)
    {
        unit.SetSelected(true);
        selectedUnits.Add(unit);
    }

    private void DeselectAll()
    {
        foreach (Unit unit in selectedUnits)
            if (IsInstanceValid(unit))
                unit.SetSelected(false);

        selectedUnits.Clear();
    }

    private IEnumerable<Unit> GetAllUnits()
    {
        foreach (Node node in GetTree().GetNodesInGroup("PlayerUnit"))
            if (node is Unit unit)
                yield return unit;
    }

    private Vector2 ScreenToWorld(Vector2 screenPos)
	{
		return GetViewport().GetCanvasTransform().AffineInverse() * screenPos;
	}

	private Vector2 WorldToScreen(Vector2 worldPos)
	{
		return GetViewport().GetCanvasTransform() * worldPos;
	}
}
