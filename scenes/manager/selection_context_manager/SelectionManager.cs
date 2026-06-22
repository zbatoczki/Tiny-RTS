using System.Collections.Generic;
using Game.Buildings;
using Game.Context;
using Game.Context.Actions;
using Game.Groups;
using Game.InputMap;
using Game.Resources;
using Game.Selection;
using Game.Units;
using Godot;
using Godot.Collections;

namespace Game.SelectionManager;

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
    [Signal] public delegate void UnitsSelectedEventHandler(Array<Unit> units);
    [Signal] public delegate void BuildingSelectedEventHandler(Building building);
    [Signal] public delegate void ResourceSelectedEventHandler(ResourceNode resource);
    [Signal] public delegate void SelectionClearedEventHandler();

    [Export] TreeTileMapLayerManager treeTileMapLayer = null!;
    [Export] BuildingManager buildingManager;

    private Vector2 _dragStart;
    private readonly List<Unit> _selectedUnits = [];
    private Building _selectedBuilding;
    private ResourceNode _selectedResource;

    private ReferenceRect _selectionBox = null;
    private ContextActionHandler _contextHandler = null;
    private SelectionContext _currentContext = SelectionContext.Empty;

    /// <summary>
    /// A command based on selection from action panel.
    /// </summary>
    public enum PendingCommand { None, Move, Attack }
    private PendingCommand _pendingCommand = PendingCommand.None;

    // Small threshold: drags shorter than this are treated as clicks.
    private const float CLICK_THRESHOLD = 6f;

    public override void _Ready()
    {
        _selectionBox = GetNode<ReferenceRect>("SelectionBox");
        _selectionBox.Visible = false;

        _contextHandler = new ContextActionHandler(this, treeTileMapLayer);

        buildingManager.BuildingPlaced += OnBuildingPlaced;
    }

    private void OnBuildingPlaced(Building placedBuilding)
    {
        // If workers are selected, send them straight to construct the building just placed.
        var buildAction = new BuildAction();
        if (!buildAction.CanHandle(_currentContext, placedBuilding)) return;

        buildAction.Execute(_currentContext, placedBuilding, placedBuilding.GlobalPosition);
    }


    public override void _UnhandledInput(InputEvent evt)
    {
        HandleLeftClickDragStart(evt);
        HandleLeftClickDragHold();
        HandleLeftClickRelease(evt);
        HandleRightClick(evt);
    }

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
        _selectionBox.Visible = _selectionBox.Size.Length() > CLICK_THRESHOLD;
    }

    private void HandleLeftClickRelease(InputEvent evt)
    {
        if (!evt.IsActionReleased(InputMapping.LEFT_CLICK)) return;

        _selectionBox.Visible = false;
        Vector2 endPosition = GetGlobalMousePosition();
        float dragDistance = _dragStart.DistanceTo(endPosition);

        if (_pendingCommand != PendingCommand.None)
        {
            if (dragDistance <= CLICK_THRESHOLD)
                ExecutePendingCommand(endPosition);
            _pendingCommand = PendingCommand.None;
            return;
        }

        if (dragDistance <= CLICK_THRESHOLD)
            HandleSingleClick(_dragStart);
        else
            HandleBoxSelection(_dragStart, endPosition);
    }


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

                case ResourceNode resource:
                    SelectResource(resource);
                    return;
            }
        }

        // Nothing hit — clear selection.
        ClearSelection();
    }

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

    private void HandleRightClick(InputEvent evt)
    {
        if (!evt.IsActionPressed(InputMapping.RIGHT_CLICK)) return;
        if (evt.IsActionPressed(InputMapping.LEFT_CLICK)) return;   // ignore while dragging

        if (_pendingCommand != PendingCommand.None)
        {
            _pendingCommand = PendingCommand.None;
            return;
        }

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

    private void SelectResource(ResourceNode resource)
    {
        ClearSelection();

        _selectedResource = resource;

        EmitSignal(SignalName.ResourceSelected, _selectedResource);
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

        _selectedResource = null;

        _pendingCommand = PendingCommand.None;
        _currentContext = SelectionContext.Empty;
        EmitSignal(SignalName.SelectionCleared);
    }

	#endregion

    #region UNIT COMMANDS (driven by the action panel)

    /// <summary>Arms a move order; the next left-click moves the selection to that point.</summary>
    public void BeginMoveCommand()
    {
        if (_currentContext.HasUnits) _pendingCommand = PendingCommand.Move;
    }

    /// <summary>Arms an attack order; the next left-click attacks the target there (or attack-moves).</summary>
    public void BeginAttackCommand()
    {
        if (_currentContext.HasUnits) _pendingCommand = PendingCommand.Attack;
    }

    /// <summary>Immediately halts every selected unit.</summary>
    public void StopSelectedUnits()
    {
        _selectedUnits.ForEach(unit => unit.Stop());
    }

    private void ExecutePendingCommand(Vector2 worldPosition)
    {
        if (!_currentContext.HasUnits) return;

        switch (_pendingCommand)
        {
            case PendingCommand.Move:
                new MoveAction().Execute(_currentContext, null, worldPosition);
                break;

            case PendingCommand.Attack:
                GodotObject target = GetTopColliderAt(worldPosition);
                if (target is Unit enemy && enemy.IsInGroup(GlobalGroups.ENEMY_UNIT))
                    new AttackAction().Execute(_currentContext, enemy, worldPosition);
                else
                    // Attack-move: no enemy under the cursor, so just move there.
                    new MoveAction().Execute(_currentContext, null, worldPosition);
                break;
        }
    }

    private GodotObject GetTopColliderAt(Vector2 worldPosition)
    {
        foreach (Dictionary hit in QueryPointAt(worldPosition))
            if (hit.TryGetValue("collider", out Variant v))
                return (GodotObject)v.Obj;
        return null;
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
        _currentContext = _selectedUnits.Count > 0 ? new SelectionContext(_selectedUnits) : SelectionContext.Empty;
    }

	#endregion
}