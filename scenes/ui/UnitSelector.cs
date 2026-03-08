using System.Linq;
using Game.Units;
using Godot;

namespace Game.UI;

public partial class UnitSelector : Control
{
	private const int UNITS_LAYER = 1 << 1;
	private readonly StringName LEFT_CLICK = "left_click";

	[Export]
	private Color borderColor;
	[Export]
	private float borderWidth;

	private bool selecting = false;
	private Vector2 dragStart;
	private Rect2 selectBox;
	private System.Collections.Generic.Dictionary<int, Unit> selectedUnits = [];

    public override void _UnhandledInput(InputEvent inputEvent)
    {
		Vector2 mousePosition = GetViewport().GetMousePosition();
        if(inputEvent.IsActionPressed(LEFT_CLICK))
		{			
			selecting = true;
			dragStart = mousePosition;
		}
		else if(inputEvent.IsActionReleased(LEFT_CLICK))
		{
			selecting = false;
			if (dragStart.IsEqualApprox(mousePosition))
				SelectSingleUnit();
			else
				UpdateSelectedUnits();
			
			QueueRedraw();
		}
		else if(selecting && inputEvent is InputEventMouseMotion mouseMotion)
		{
			GenerateSelectionBox(mousePosition);
		}
    }

    public override void _Draw()
    {
        if(!selecting) return;

		DrawRect(selectBox, borderColor, false, borderWidth);
    }

	private void GenerateSelectionBox(Vector2 mousePosition)
	{
		var xMin = Mathf.Min(dragStart.X, mousePosition.X);
		var yMin = Mathf.Min(dragStart.Y, mousePosition.Y);
		var width = Mathf.Max(dragStart.X, mousePosition.X) - xMin;
		var height = Mathf.Max(dragStart.Y, mousePosition.Y) - yMin;

		selectBox = new Rect2(xMin, yMin, width, height);

		UpdateSelectedUnits();
		QueueRedraw();
	}

	private void UpdateSelectedUnits()
	{
		var playerUnits = GetTree().GetNodesInGroup("PlayerUnit").Cast<Unit>();
		foreach(var unit in playerUnits)
		{
			if (unit.IsInsideSelectionBox(selectBox))
				unit.Select();
			else
				unit.Deselect();
		}
	}

	private void SelectSingleUnit()
	{
		PhysicsDirectSpaceState2D space = GetWorld2D().DirectSpaceState;


        var query = new PhysicsPointQueryParameters2D
		{
			Position = GetGlobalMousePosition(),
			CollideWithAreas = false,
			CollideWithBodies = true,
			CollisionMask = UNITS_LAYER
		};

		var results = space.IntersectPoint(query);

		DeselectAllUnits();
		GD.Print(results.Count);
        if (results.Count == 0)
            return;

        var collider = results[0]["collider"].As<Node>();

        if (collider is Unit unitHit)
        {
            unitHit.Select();
        }
	}

	private void DeselectAllUnits()
	{
		var playerUnits = GetTree().GetNodesInGroup("PlayerUnit").Cast<Unit>();
        foreach (var unit in playerUnits)
		{
			unit.Deselect();
		}   
	}


	
}
