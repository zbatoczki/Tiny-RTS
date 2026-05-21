using Game.Component;
using Game.Resources;
using Game.Resources.Gathering;
using Game.Units;
using Godot;
using System.Collections.Generic;
namespace Game.Buildings;

public partial class GoldMine : ResourceNode
{
	private Sprite2D inactiveSprite;
	private Sprite2D activeSprite;
	private Sprite2D destroyedSprite;


	private enum SpriteStates
	{
		INACTIVE, ACTIVE, DESTRYOED
	}

	private List<Worker> workersInMine = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeResource();
		inactiveSprite = GetNode<Sprite2D>("Inactive");
		activeSprite = GetNode<Sprite2D>("Active");
		destroyedSprite = GetNode<Sprite2D>("Destroyed");

		ResourceGathered += OnResourceGathered;
		ResourceDepleted += OnResourceDepleted;

		CellCoordinates = (Vector2I)GlobalPosition;
	}

    private void OnResourceDepleted(Vector2I _)
    {
		GD.Print($"Gold mine at {CellCoordinates} is depleted.");
        SetActiveSprite(SpriteStates.DESTRYOED);
    }


    private void OnResourceGathered(Vector2I cellCoordinates, int amount)
    {
		if(workersInMine.Count == 0)
		{
			SetActiveSprite(SpriteStates.INACTIVE);
		}
    }

	public void EnterMine(Worker worker)
	{
		workersInMine.Add(worker);
		SetActiveSprite(SpriteStates.ACTIVE);
	}

	private void SetActiveSprite(SpriteStates state)
	{
		switch (state)
		{
			case SpriteStates.INACTIVE:
				inactiveSprite.Visible = true;
				activeSprite.Visible = false;
				destroyedSprite.Visible = false;
				break;

			case SpriteStates.ACTIVE:
				inactiveSprite.Visible = false;
				activeSprite.Visible = true;
				destroyedSprite.Visible = false;
				break;

			case SpriteStates.DESTRYOED:
				inactiveSprite.Visible = false;
				activeSprite.Visible = false;
				destroyedSprite.Visible = true;
				break;
		}
	}
}
