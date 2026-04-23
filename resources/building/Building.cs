using Game.Autoload;
using Game.Component;
using Game.Globals;
using Godot;

namespace Game.Resources.Building;

public partial class Building : StaticBody2D
{
	[Export] public Faction Faction;
    [Export] public float MaxHealth = 500f;

    public float CurrentHealth {get; protected set;}

    private HealthComponent healthBar;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        healthBar = GetNodeOrNull<HealthComponent>(nameof(HealthComponent));
        if(healthBar != null)
        {
            healthBar.SetMaxValue(MaxHealth);
            healthBar.SetCurrentHealth(MaxHealth);
            AddToGroup("Buildings");
            GameManager.Instance.RegisterBuilding(this);
            OnReady();
        }
    }

    public virtual void OnReady() { }

    public virtual void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        healthBar?.SetCurrentHealth(CurrentHealth);
        if(CurrentHealth <= 0)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        GameManager.Instance.UnregisterBuilding(this);
        QueueFree();
    }
}
