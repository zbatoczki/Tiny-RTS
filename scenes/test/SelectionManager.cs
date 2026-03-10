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
