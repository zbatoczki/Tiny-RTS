using Game.Buildings;
using Godot;
using System;

public partial class UI : CanvasLayer
{
	[Export] private Castle castle;
	[Export] private Barracks barracks;
	
	private Button trainWorkerButton;
	private Button trainWarriorButton;

	public override void _Ready()
	{
		trainWorkerButton = GetNode<Button>("TrainWorkerButton");
		trainWorkerButton.Pressed += OnTrainWorkerButtonPressed;
		

		trainWarriorButton = GetNode<Button>("TrainWarriorButton");
		trainWarriorButton.Pressed += OnTrainWarriorButtonPressed;
	}

    private void OnTrainWarriorButtonPressed()
    {
        barracks.TrainUnit(Game.Globals.UnitTypes.Warrior);
    }


    private void OnTrainWorkerButtonPressed()
    {
        castle.TrainUnit(Game.Globals.UnitTypes.Worker);
    }

}
