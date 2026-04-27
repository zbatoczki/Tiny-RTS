using Game.Buildings;
using Godot;
using System;

public partial class TrainWorkerButton : Button
{
	[Export] private Castle castle;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += OnButtonPressed;
	}

    private void OnButtonPressed()
    {
        castle.TrainUnit();
    }

}
