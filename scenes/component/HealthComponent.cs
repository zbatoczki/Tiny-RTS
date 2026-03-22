using Godot;
namespace Game.Component;
public partial class HealthComponent : Node2D
{
	private ProgressBar healthProgressBar;

	private Button healthUpButton;
	private Button healthDownButton;

	public override void _Ready()
	{
		healthProgressBar = GetNode<ProgressBar>("%HealthProgressBar");
		Visible = false;
	}

	public void SetMaxValue(int health)
	{
		healthProgressBar.MaxValue = health;
		SetHealthBarVisibility();
	}

	public void SetCurrentHealth(int currentHealth)
	{
		healthProgressBar.Value = currentHealth;
		SetHealthBarVisibility();
	}

	public void AddToCurrentHealth(int healthToAdd)
	{
		healthProgressBar.Value += healthToAdd;
		SetHealthBarVisibility();
	}

	private void SetHealthBarVisibility()
	{
		Visible = healthProgressBar.Value != healthProgressBar.MaxValue;
	}


}
