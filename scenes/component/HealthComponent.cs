using Godot;
namespace Game.Component;
public partial class HealthComponent : Node2D
{
	private ProgressBar healthProgressBar;

	public override void _Ready()
	{
		healthProgressBar = GetNode<ProgressBar>("%HealthProgressBar");
		Visible = false;
		healthProgressBar.Value = healthProgressBar.MaxValue;
	}

	public void SetMaxValue(float health)
	{
		healthProgressBar.MaxValue = health;
		SetHealthBarVisibility();
	}

	public void SetCurrentHealth(float currentHealth)
	{
		healthProgressBar.Value = currentHealth;
		SetHealthBarVisibility();
	}

	public void AddToCurrentHealth(float healthToAdd)
	{
		healthProgressBar.Value += healthToAdd;
		SetHealthBarVisibility();
	}

	private void SetHealthBarVisibility()
	{
		Visible = healthProgressBar.Value != healthProgressBar.MaxValue;
	}


}
