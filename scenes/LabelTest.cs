using Game.Units;
using Godot;

public partial class LabelTest : Label
{
	[Export] private Unit worker;
	[Export] private Unit skeleton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = $"Worker State: {worker.GetCurrentState()} | Skelton State: {skeleton.GetCurrentState()}";
	}
}
