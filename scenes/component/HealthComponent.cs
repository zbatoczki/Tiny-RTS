using Godot;

public partial class HealthComponent : Node2D
{
	private ProgressBar healthProgressBar;

	private Button healthUpButton;
	private Button healthDownButton;

	public override void _Ready()
	{
		healthProgressBar = GetNode<ProgressBar>("%HealthProgressBar");
	}

	public void SetMaxValue(int health)
	{
		healthProgressBar.MaxValue = health;
	}

	public void SetCurrentHealth(int currentHealth)
	{
		healthProgressBar.Value = currentHealth;
	}

	public void AddToCurrentHealth(int healthToAdd)
	{
		healthProgressBar.Value += healthToAdd;
	}


}
