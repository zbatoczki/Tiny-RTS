using System.Collections.Generic;
using Game.Autoload;
using Game.Component;
using Game.Globals;
using Game.Resources.Building;
using Game.Units;
using Godot;

namespace Game.Buildings;

public abstract partial class Building : StaticBody2D
{
    [Export] public BuildingResource BuildingResource;
	[Export] public Faction Faction;
    [Export] public float MaxHealth = 500f;
    [Export] public float TrainTime = 3f;

    public float CurrentHealth {get; protected set;}

    private HealthComponent healthBar;
    private Timer timer;
    private Sprite2D selectionRing;
    private CollisionShape2D bodyCollisionShape;

    /// <summary>
    /// World position of the building's footprint center, taken from its body collision shape.
    /// A shape node's origin is its geometric center, so this is the true middle of the building.
    /// </summary>
    public Vector2 CenterPosition
    {
        get
        {
            bodyCollisionShape ??= GetBodyCollisionShape();
            return bodyCollisionShape?.GlobalPosition ?? GlobalPosition;
        }
    }

    protected readonly Queue<(PackedScene unitScene, float waitTime)> queue = new();

    public void OnReady()
    {
        CurrentHealth = MaxHealth;
        healthBar = GetNodeOrNull<HealthComponent>(nameof(HealthComponent));

        if(healthBar != null)
        {
            healthBar.SetMaxValue(MaxHealth);
            healthBar.SetCurrentHealth(MaxHealth);      
        }

        AddToGroup("Buildings");
        GameManager.Instance.RegisterBuilding(this);

        timer = GetNode<Timer>(nameof(Timer));
        timer.WaitTime = TrainTime;
        timer.Timeout += OnTimeout;

        selectionRing = GetNode<Sprite2D>("SelectionRing");
        selectionRing.Visible = false;

        if(BuildingResource != null)
        {
            GD.Print($"Description: {BuildingResource.Description}");
        }
    }

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
        SpawnUnit(queue.Dequeue().unitScene);
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

    public abstract bool TrainUnit(UnitTypes unitType);

    private void SpawnUnit(PackedScene scene)
    {
        if(scene == null) return;
        var unit = scene.Instantiate<Unit>();
        unit.stats.Faction = Faction;
        unit.GlobalPosition = GlobalPosition + new Vector2(GD.RandRange(-128, 128), 96);
        GetParent().AddChild(unit);
    }

    internal virtual void SetSelected(bool selected)
    {
        selectionRing.Visible = selected;
        OnSelectionChanged(selected);
    }

     protected virtual void OnSelectionChanged(bool selected)
    {
        GD.Print($"{Name} selected={selected}");
    }

    /// <summary>Returns the building body's own collision shape (ignores shapes nested in child components).</summary>
    private CollisionShape2D GetBodyCollisionShape()
    {
        foreach (var child in GetChildren())
        {
            if (child is CollisionShape2D shape)
                return shape;
        }
        return null;
    }
}
