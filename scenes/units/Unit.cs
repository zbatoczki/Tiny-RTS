using Game.Autoload;
using Game.InputMap;
using Godot;

namespace Game.Units;

public partial class Unit : CharacterBody2D
{
	[Export]
	private Color borderColor;
	private HealthComponent healthComponent;
	private Rect2 selectedRect;
	private int selectedRectWidth = 1;

	private bool _selected = false;
	private bool Selected 
	{
		get => _selected;
		set
		{
			_selected = value;
			if (_selected)
			{
				selectedRect = new Rect2(new Vector2(-8,8), new Vector2(16,16));
				selectedRectWidth = 2;
			}
			else
			{
				selectedRect = new Rect2(0,0,0,0);
				selectedRectWidth = 0;
			}
			QueueRedraw();
		}
	}

    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));
    }

    public override void _Draw()
    {
        DrawRect(selectedRect, borderColor, filled: false, selectedRectWidth);
    }

    public override void _InputEvent(Viewport viewport, InputEvent evt, int shapeIdx)
    {
		if (evt.IsActionPressed(InputMapping.LEFT_CLICK))
		{
			Select();
			foreach(var unit in UnitManager.Instance.selectedUnits)
			{
				if(unit != this)
					unit.Deselect();
			}
			UnitManager.Instance.selectedUnits = [this];
		}
    }

	public void Select()
	{
		Selected = true;
		healthComponent.Visible = true;
		AddToGroup("SelectedUnits");
	}

	public void Deselect()
	{
		Selected = false;
		healthComponent.Visible = false;
		RemoveFromGroup("SelectedUnits");
	}
}
