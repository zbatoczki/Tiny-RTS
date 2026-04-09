using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Game.Buildings;
using Game.Groups;
using Game.InputMap;
using Game.Resources;
using Game.Resources.Gathering;
using Game.Units;
using Godot;
using Godot.Collections;

namespace Game.Test;

public partial class SelectionManager : Node2D
{
	[Export] TileMapLayer treeTileMapLayer;
	[Export] TreeTileMapLayerManager treeTileMapLayerManager;

	private Vector2 startPosition;
	private List<Unit> selectedUnits = [];
	private ReferenceRect selectionBox;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		selectionBox = GetNode<ReferenceRect>("SelectionBox");
		selectionBox.Visible = false;
	}

    public override void _Input(InputEvent evt)
    {
		if (evt.IsActionPressed(InputMapping.LEFT_CLICK))
		{
			startPosition = GetGlobalMousePosition();
		}

		if(Input.IsActionPressed(InputMapping.LEFT_CLICK))
		{
			selectionBox.Visible = true;
			Vector2 currentMousePosition = GetGlobalMousePosition();
			Vector2 upperLeft = new(
				Mathf.Min(startPosition.X, currentMousePosition.X),
				Mathf.Min(startPosition.Y, currentMousePosition.Y)
			);
			selectionBox.GlobalPosition = upperLeft;
			selectionBox.Size = (currentMousePosition - startPosition).Abs();
		}

		if (evt.IsActionReleased(InputMapping.LEFT_CLICK))
		{
			DeselectUnits();
			selectionBox.Visible = false;

			var endPosition = GetGlobalMousePosition();
			GD.Print($"Start Position:{startPosition}|End Position:{endPosition}");
			
			var rect = new Rect2(startPosition, Vector2.Zero).Expand(endPosition);

			Array<Dictionary> results = GetOverlappingUnits(rect);
			foreach(Dictionary r in results)
			{
				Unit unit = r["collider"].As<Unit>();
				unit.SetSelected(true);
				selectedUnits.Add(unit);
				unit.UnitDied += OnUnitDied;
			}
		}

		if (evt.IsActionPressed(InputMapping.RIGHT_CLICK) && !Input.IsActionPressed(InputMapping.LEFT_CLICK))
		{
			//get mouse position
			Vector2 mousePosition = GetGlobalMousePosition();
			//check what is at mouse position
			Array<Dictionary> results = CheckAtMousePosition(mousePosition);
			//TODO handle action based on what is at position and what units are selected
			
			foreach(var item in results)
			{
				GD.Print(item["collider"].Obj);
				if (item["collider"].Obj is Unit n && n.IsInGroup(GlobalGroups.ENEMY_UNIT))
				{
					selectedUnits.ForEach(unit => unit.AttackTarget = n);
				}
				else if (item["collider"].Obj is TileMapLayer tml && tml == treeTileMapLayer)
				{
					var workerUnits = selectedUnits.OfType<Worker>().ToList();
					if(workerUnits.Count == 0) continue;

					Vector2 localPosition = treeTileMapLayer.ToLocal(mousePosition);
					Vector2I cellPosition = treeTileMapLayer.LocalToMap(localPosition);
					GatheringResource treeData = treeTileMapLayerManager.GetTreeAt(cellPosition);
					if(treeData != null && !treeData.IsDepleted)
					{
						GD.Print(treeData);
						GD.Print($"Clicked tree at cell {cellPosition}, amount of wood: {treeData.CurrentCharges}");
						workerUnits.ForEach(unit =>
						{
							unit.GatheringResourceTarget = treeData;
							GD.Print($"unit tree target: {treeData.Name}");
						});
					}
					else
					{
						GD.Print("No tree found");
					}
				}
				else if (item["collider"].Obj is StaticBody2D node && node.GetParent() is GoldMine goldmine)
				{
					GD.Print("GoldMine detected found");
					var workerUnits = selectedUnits.OfType<Worker>().ToList();
					if(workerUnits.Count == 0) continue;

					GatheringResource goldMineData = goldmine._GatheringResource;
					if(goldMineData != null && !goldMineData.IsDepleted)
					{
						workerUnits.ForEach(unit =>
						{
							unit.GatheringResourceTarget = goldMineData;
							GD.Print($"unit target: {goldMineData.Name}");
						});
					}
				}
			}
			if (results.Count == 0)
			{
				selectedUnits.ForEach(unit => unit.AttackTarget = null);
			}
			MoveUnitsToPosition(mousePosition);
		}
    }

	private Array<Dictionary> CheckAtMousePosition(Vector2 mousePosition)
	{
		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
		var query = new PhysicsPointQueryParameters2D()
		{
			Position = mousePosition
		};

		return spaceState.IntersectPoint(query);
	} 

	private void DeselectUnits()
	{
		selectedUnits.ForEach(unit => unit.SetSelected(false));
		selectedUnits.Clear();
	}

	private Array<Dictionary> GetOverlappingUnits(Rect2 rect)
	{
		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

        var query = new PhysicsShapeQueryParameters2D()
		{
			Shape = new RectangleShape2D(){ Size = rect.Size },
			Transform = new Transform2D(0, rect.GetCenter()),
			CollisionMask = 2
		};

        return spaceState.IntersectShape(query);
	}

	private List<Vector2> GetTargetPositions(Vector2 mousePosition)
	{
		var positions = new List<Vector2>();

		int cols = Mathf.CeilToInt(Mathf.Sqrt(selectedUnits.Count));
		int rows = Mathf.CeilToInt((float)selectedUnits.Count / cols);

		float spacing = 64f;

		// Total grid size
		float gridWidth = (cols - 1) * spacing;
		float gridHeight = (rows - 1) * spacing;

		// Top-left corner of the grid, so that mousePosition ends up in the center
		Vector2 origin = mousePosition - new Vector2(gridWidth / 2f, gridHeight / 2f);

		for (int i = 0; i < selectedUnits.Count; i++)
		{
			int col = i % cols;
			int row = i / cols;
			positions.Add(origin + new Vector2(col * spacing, row * spacing));
		}

		return positions;
	}

	private void MoveUnitsToPosition(Vector2 mousePosition)
	{
		List<Vector2> targetPositions = GetTargetPositions(mousePosition);
		int targetPositionIndex = 0;
		foreach(Unit unit in selectedUnits)
		{
			unit.MoveTo(targetPositions[targetPositionIndex]);
			targetPositionIndex++;
		}
	}

	private void OnUnitDied(Unit unit)
	{
		unit.UnitDied -= OnUnitDied;
		selectedUnits.Remove(unit);
	}

}
