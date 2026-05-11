using Game.Buildings;
using Game.Autoload;
using Godot;

public partial class UI : CanvasLayer
{
	[Export] private Castle castle;
	[Export] private Barracks barracks;
    [Export] private ArcheryRange archery;
	
	private Button trainWorkerButton;
	private Button trainWarriorButton;
	private Button trainSpearmanButton;
	private Button trainArcherButton;
	private Button resetButton;

	public override void _Ready()
	{
		trainWorkerButton = GetNode<Button>("%TrainWorkerButton");
		trainWorkerButton.Pressed += OnTrainWorkerButtonPressed;
		

		trainWarriorButton = GetNode<Button>("%TrainWarriorButton");
		trainWarriorButton.Pressed += OnTrainWarriorButtonPressed;

		trainSpearmanButton = GetNode<Button>("%TrainSpearmanButton");
		trainSpearmanButton.Pressed += OnTrainSpearmanButtonPressed;

		trainArcherButton = GetNode<Button>("%TrainArcherButton");
		trainArcherButton.Pressed += OnTrainArcherButtonPressed;

		resetButton = GetNode<Button>("ResetButton");
		resetButton.Pressed += OnResetButtonPressed;
	}

    private void OnResetButtonPressed()
    {
        ResourceManager.Instance.InitializeResources();
        GetTree().ReloadCurrentScene();
    }


    private void OnTrainArcherButtonPressed()
    {
        archery.TrainUnit(Game.Globals.UnitTypes.Archer);
    }


    private void OnTrainSpearmanButtonPressed()
    {
        barracks.TrainUnit(Game.Globals.UnitTypes.Spearman);
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
