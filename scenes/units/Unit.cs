using System.Numerics;
using Godot;

namespace Game.Units;

public partial class Unit : CharacterBody2D
{
	private HealthComponent healthComponent;
    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));
    }

	public bool IsInsideSelectionBox(Rect2 selectionBox)
	{
		return selectionBox.HasPoint(GetGlobalTransformWithCanvas().Origin);
	}

	public void Select()
	{
		healthComponent.Visible = true;
		AddToGroup("SelectedUnits");
	}

	public void Deselect()
	{
		healthComponent.Visible = false;
		RemoveFromGroup("SelectedUnits");
	}
}
