using System;
using System.Collections.Generic;
using System.Linq;
using Game.Units;
using Godot;

namespace Game.Autoload;

public partial class UnitManager : Node2D
{
	public static UnitManager Instance { get; private set; }

	public HashSet<Unit> selectedUnits {get; set;} = [];
	public Rect2? SelectedRect
	{
		get;
		set
		{
			field = value;
			CheckUnit();
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}

	public override void _Ready()
	{
		GD.Print("UnitManager ready");
	}


	public void CheckUnit()
	{
		var units = GetTree().GetNodesInGroup("PlayerUnit").Cast<Unit>();
		foreach (var unit in units)
		{
			if (SelectedRect.HasValue && SelectedRect.Value.HasPoint(unit.GlobalPosition))
			{
				unit.Select();
				selectedUnits.Add(unit);
			}
			else
			{
				unit.Deselect();
			}
		}
	}

	public void MoveToPosition(TileMapLayer tileMapLayer, Vector2I tilePosition)
	{
		List<Vector2I> formation = GetFormation(tilePosition);
		var listIndex = 0;
		foreach(var unit in selectedUnits)
		{
			unit.GlobalPosition = tileMapLayer.MapToLocal(formation[listIndex]);
			listIndex++;
		}
	}

	private List<Vector2I> GetFormation(Vector2I tilePosition)
	{
		int unitCount = selectedUnits.Count;
		List<Vector2I> formation = new(unitCount);
		var formationSize = (int)Math.Ceiling(Math.Sqrt(unitCount));

		var index = 0;
		for(int x = -formationSize /2; x <= formationSize/2; x++)
		{
			for(int y = -formationSize /2; y <= formationSize/2; y++)
			{
				if(index < unitCount)
				{
					formation.Add(tilePosition + new Vector2I(x, y));
					index++;
				}
				else break;
			}
		}
		return formation;
	}
}
