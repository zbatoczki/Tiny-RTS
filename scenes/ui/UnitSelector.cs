using System.Linq;
using Game.Autoload;
using Game.InputMap;
using Game.Units;
using Godot;

namespace Game.UI;

public partial class UnitSelector : Control
{
	private const float DRAG_THRESHOLD = 10f;

	[Export]
	private Color borderColor;
	[Export]
	private float borderWidth;
	[Export]
	private TileMapLayer tileMapLayer;

	private float defaultWidth;

	private bool selecting = false;
	private Vector2 dragStart;
	private Vector2 dragEnd;
	private Rect2 selectBox;

    public override void _Ready()
    {
        defaultWidth = borderWidth;
    }


    public override void _UnhandledInput(InputEvent inputEvent)
    {
		Vector2 mousePosition = GetGlobalMousePosition();
        if(inputEvent.IsActionPressed(InputMapping.LEFT_CLICK))
		{
			borderWidth = defaultWidth;
			selecting = true;
			dragStart = mousePosition;
			dragEnd = mousePosition;
		}
		else if(inputEvent.IsActionReleased(InputMapping.LEFT_CLICK))
		{
			borderWidth = 0;
			selecting = false;

			float dragDistance = dragStart.DistanceTo(mousePosition);
			if(dragDistance < DRAG_THRESHOLD)
			{
				UnitManager.Instance.SelectedRect = null;
			}

			dragStart = Vector2.Zero;
			dragEnd = Vector2.Zero;
			QueueRedraw();
		}
		
		if(selecting && inputEvent is InputEventMouseMotion)
		{
			dragEnd = mousePosition;
			QueueRedraw();
			UnitManager.Instance.SelectedRect = selectBox;
		}
		if (inputEvent.IsActionPressed(InputMapping.RIGHT_CLICK))
		{
			UnitManager.Instance.MoveToPosition(tileMapLayer, GetTilePosition(mousePosition));
		}
    }

    public override void _Draw()
    {
		Vector2 rectPosition = dragStart;
		Vector2 rectSize = dragEnd - dragStart;
		
		if(rectSize.X < 0)
		{
			rectPosition.X += rectSize.X;			
		}
		if(rectSize.Y < 0)
		{
			rectPosition.Y += rectSize.Y;			
		}
		rectSize = rectSize.Abs();

		selectBox = new Rect2(rectPosition, rectSize);

		DrawRect(selectBox, borderColor, filled: false, borderWidth);
    }

	private Vector2I GetTilePosition(Vector2 position)
	{
		Vector2 localposition = tileMapLayer.ToLocal(position);
		Vector2I tilePosition = tileMapLayer.LocalToMap(localposition);
		return tilePosition;
	}

	
}
