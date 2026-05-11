using System.Collections.Generic;
using Game.Buildings;
using Game.Context;
using Game.InputMap;
using Game.Resources;
using Game.Selection;
using Game.Units;
using Godot;
using Godot.Collections;

namespace Game.Test;

/// <summary>
/// Handles all player selection input and fires events when the selection changes.
/// Right-click context actions are delegated entirely to <see cref="ContextActionHandler"/>.
///
/// Listen to the signals below to drive UI changes (e.g. show the building training panel):
///   - UnitsSelected(units)
///   - BuildingSelected(building)
///   - SelectionCleared()
/// </summary>
public partial class SelectionManager : Node2D
{
    // -------------------------------------------------------------------------
    // Signals — connect these in the UI layer
    // -------------------------------------------------------------------------

    [Signal] public delegate void UnitsSelectedEventHandler(Array<Unit> units);
    [Signal] public delegate void BuildingSelectedEventHandler(Building building);
    [Signal] public delegate void SelectionClearedEventHandler();

    // -------------------------------------------------------------------------
    // Exports
    // -------------------------------------------------------------------------

    [Export] TreeTileMapLayerManager treeTileMapLayer = null!;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _dragStart;
    private readonly List<Unit> _selectedUnits = [];
    private Building _selectedBuilding;

    private ReferenceRect _selectionBox = null!;
    private ContextActionHandler _contextHandler = null!;
    private SelectionContext _currentContext = SelectionContext.Empty;

    // Small threshold: drags shorter than this are treated as clicks.
    private const float ClickThreshold = 6f;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    public override void _Ready()
    {
        _selectionBox = GetNode<ReferenceRect>("SelectionBox");
        _selectionBox.Visible = false;

        _contextHandler = new ContextActionHandler(this, treeTileMapLayer);
    }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------

    public override void _Input(InputEvent evt)
    {
        HandleLeftClickDragStart(evt);
        HandleLeftClickDragHold();
        HandleLeftClickRelease(evt);
        HandleRightClick(evt);
    }

    // -------------------------------------------------------------------------
    // Left-click drag — box selection
    // -------------------------------------------------------------------------

    private void HandleLeftClickDragStart(InputEvent evt)
    {
        if (!evt.IsActionPressed(InputMapping.LEFT_CLICK)) return;
        _dragStart = GetGlobalMousePosition();
    }

    private void HandleLeftClickDragHold()
    {
        if (!Input.IsActionPressed(InputMapping.LEFT_CLICK)) return;

        Vector2 mouse = GetGlobalMousePosition();
        Vector2 upperLeft = new(
            Mathf.Min(_dragStart.X, mouse.X),
            Mathf.Min(_dragStart.Y, mouse.Y));

        _selectionBox.GlobalPosition = upperLeft;
        _selectionBox.Size = (mouse - _dragStart).Abs();
        _selectionBox.Visible = _selectionBox.Size.Length() > ClickThreshold;
    }

    private void HandleLeftClickRelease(InputEvent evt)
    {
        if (!evt.IsActionReleased(InputMapping.LEFT_CLICK)) return;

        _selectionBox.Visible = false;
        Vector2 endPosition = GetGlobalMousePosition();
        float dragDistance = _dragStart.DistanceTo(endPosition);

        if (dragDistance <= ClickThreshold)
            HandleSingleClick(_dragStart);
        else
            HandleBoxSelection(_dragStart, endPosition);
    }

    // -------------------------------------------------------------------------
    // Single click — selects a unit, building, or clears selection
    // -------------------------------------------------------------------------

    private void HandleSingleClick(Vector2 worldPosition)
    {
        Array<Dictionary> hits = QueryPointAt(worldPosition);

        // Try to find a selectable entity in hit results.
        foreach (Dictionary hit in hits)
        {
            if (!hit.TryGetValue("collider", out Variant colliderVariant)) continue;

            switch (colliderVariant.Obj)
            {
                case Building building:
                    SelectBuilding(building);
                    return;

                case Unit unit:
                    SelectUnits([unit]);
                    return;
            }
        }

        // Nothing hit — clear selection.
        ClearSelection();
    }

    // -------------------------------------------------------------------------
    // Box selection — selects all units inside the drag rect
    // -------------------------------------------------------------------------

    private void HandleBoxSelection(Vector2 start, Vector2 end)
    {
        ClearSelection();

        var rect = new Rect2(start, Vector2.Zero).Expand(end);
        Array<Dictionary> hits = QueryShapeAt(rect);

        var units = new List<Unit>();
        foreach (Dictionary hit in hits)
        {
            if (hit.TryGetValue("collider", out Variant v) && v.As<Unit>() is Unit unit)
                units.Add(unit);
        }

        if (units.Count > 0)
            SelectUnits(units);
    }

    // -------------------------------------------------------------------------
    // Right-click — delegate entirely to ContextActionHandler
    // -------------------------------------------------------------------------

    private void HandleRightClick(InputEvent evt)
    {
        if (!evt.IsActionPressed(InputMapping.RIGHT_CLICK)) return;
        if (Input.IsActionPressed(InputMapping.LEFT_CLICK)) return;   // ignore while dragging

        _contextHandler.HandleRightClick(_currentContext, GetGlobalMousePosition());
    }

    #region SELECTION HANDLERS

    private void SelectUnits(IReadOnlyList<Unit> units)
    {
        ClearSelection();

        foreach (Unit unit in units)
        {
            unit.SetSelected(true);
            _selectedUnits.Add(unit);
            unit.UnitDied += OnUnitDied;
        }

        _currentContext = new SelectionContext(_selectedUnits);
        EmitSignal(SignalName.UnitsSelected, new Array<Unit>(_selectedUnits));
    }

    private void SelectBuilding(Building building)
    {
        ClearSelection();

        _selectedBuilding = building;
        _selectedBuilding.SetSelected(true);

        _currentContext = new SelectionContext(_selectedBuilding);
        EmitSignal(SignalName.BuildingSelected, _selectedBuilding);
    }

    private void ClearSelection()
    {
        _selectedUnits.ForEach(unit =>
        {
            unit.SetSelected(false);
            unit.UnitDied -= OnUnitDied;
        });
        _selectedUnits.Clear();

        _selectedBuilding?.SetSelected(false);
        _selectedBuilding = null;

        _currentContext = SelectionContext.Empty;
        EmitSignal(SignalName.SelectionCleared);
    }

	#endregion

    #region PHYSICS QUERIES

    private Array<Dictionary> QueryPointAt(Vector2 worldPosition)
    {
        PhysicsDirectSpaceState2D space = GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D { Position = worldPosition };
        return space.IntersectPoint(query);
    }

    private Array<Dictionary> QueryShapeAt(Rect2 rect)
    {
        PhysicsDirectSpaceState2D space = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = new RectangleShape2D { Size = rect.Size },
            Transform = new Transform2D(0, rect.GetCenter()),
            CollisionMask = 2
        };
        return space.IntersectShape(query);
    }

	#endregion

    #region EVENT HANDLERS

    private void OnUnitDied(Unit unit)
    {
        unit.UnitDied -= OnUnitDied;
        _selectedUnits.Remove(unit);

        // Rebuild context after a unit is removed from selection mid-fight.
        _currentContext = _selectedUnits.Count > 0
            ? new SelectionContext(_selectedUnits)
            : SelectionContext.Empty;
    }

	#endregion
}