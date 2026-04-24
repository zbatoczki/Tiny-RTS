using System.Collections.Generic;
using Game.Autoload;
using Game.Component;
using Game.Globals;
using Game.Units;
using Godot;

namespace Game.Buildings;

public abstract partial class Building : StaticBody2D
{
	[Export] public Faction Faction;
    [Export] public float MaxHealth = 500f;
    [Export] public float TrainTime = 10f;

    public float CurrentHealth {get; protected set;}

    private HealthComponent healthBar;
    private Timer timer;

    protected readonly Queue<(PackedScene unitScene, float waitTime)> queue = new();

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
        timer = GetNode<Timer>(nameof(Timer));
        timer.WaitTime = TrainTime;
        timer.Timeout += OnTimeout;
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

    private void OnTimeout()
    {
        //spawn unit
        SpawnUnit(queue.Dequeue().unitScene);
        //get next item in queue if any and restart the timer with the queued time
        if(queue.Count > 0)
        {
            timer.WaitTime = queue.Peek().waitTime;
            timer.Start();
        }
    }

    protected void Enqueue(PackedScene scene, float time)
    {
        queue.Enqueue((scene, time));
        if(queue.Count == 1)
        {
            timer.WaitTime = time;
            timer.Start();
        }
    }

    protected abstract bool TrainUnit();

    private void SpawnUnit(PackedScene scene)
    {
        if(scene == null) return;
        var unit = scene.Instantiate<Unit>();
        unit.stats.Faction = Faction;
        unit.GlobalPosition = GlobalPosition + new Vector2(GD.RandRange(-128, 128), 96);
        GetParent().AddChild(unit);
    }
}
