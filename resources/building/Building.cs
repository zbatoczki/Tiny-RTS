using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Game.Globals;
using Game.Manager;
using Game.Resources.Building;
using Game.Resources.Unit;
using Game.Units;
using Godot;
using Godot.Collections;

namespace Game.Buildings;

public abstract partial class Building : StaticBody2D
{
    [Export] public BuildingResource BuildingResource;
    [Export] public Array<UnitResource> BuildableUnits {get; private set;}
    [Export] public float MaxHealth = 500f;
    [Export] public float TrainTime = 3f;
    [Export] private bool testAutoBuild = true;
    [Export] private float testBuildTime = 3f;
    [Export] public float BuildInterpolationTime = 0.25f;

    public float CurrentHealth {get; protected set;}
    public bool IsUnderConstruction => buildProgress < 1f;

    private HealthComponent healthBar;
    private Timer timer;
    private Sprite2D selectionRing;
    private CollisionShape2D bodyCollisionShape;
    private HashSet<Vector2I> occupiedTiles = [];


    private ShaderMaterial buildMaterial;
    private float buildProgress = 1f;
    private Tween buildTween;

    public Vector2 CenterPosition
    {
        get
        {
            bodyCollisionShape ??= GetBodyCollisionShape();
            return bodyCollisionShape?.GlobalPosition ?? GlobalPosition;
        }
    }

    protected readonly Queue<(Unit unitToSpawn, float waitTime)> queue = new();

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

        SetupBuildVisual();

        if(testAutoBuild)
        {
            RunTestBuildAnimation();
        }

        Callable.From(CalculateOccupiedCellPositions).CallDeferred();
    }

    #region GRID RELATED HELPERS

    /// <summary>
    /// Returns the building's top-left grid cell position.
    /// </summary>
    public Vector2I GetGridCellPosition()
    {
        Vector2 gridPosition = (GlobalPosition / GlobalValues.CELL_SIZE).Floor();
        return new Vector2I((int)gridPosition.X, (int) gridPosition.Y);
    }

    public HashSet<Vector2I> GetOccupiedCellPositions()
    {
        return [.. occupiedTiles];  //A copy is returned to avoid modifying the acutal set of positions.
    }

    public bool IsGridCellPositionInBuildingArea(Vector2I cellPosition)
    {
        return occupiedTiles.Contains(cellPosition);
    }

    public Rect2I GetCellArea()
    {
        var rootCell = GetGridCellPosition();
        return new Rect2I(rootCell, BuildingResource.Dimensions);
    }

    private void CalculateOccupiedCellPositions()
    {
        var gridPosition = GetGridCellPosition();
		for(int x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
		{
			for(int y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
			{
				occupiedTiles.Add(new Vector2I(x, y));
			}
		}
    }

    #endregion

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
        SpawnUnit(queue.Dequeue().unitToSpawn);
        if(queue.Count > 0)
        {
            timer.WaitTime = queue.Peek().waitTime;
            timer.Start();
        }
    }

    protected void Enqueue(Unit unitToSpawn, float time)
    {
        queue.Enqueue((unitToSpawn, time));
        if(queue.Count == 1)
        {
            timer.WaitTime = time;
            timer.Start();
        }
    }

    public bool TrainUnit(UnitResource unitResource)
    {
		int woodCost = unitResource.ResourceCosts.TryGetValue(ResourceType.Wood, out int wCost) ? wCost : 0;
		int goldCost = unitResource.ResourceCosts.TryGetValue(ResourceType.Gold, out int gCost) ? gCost : 0;
		int foodCost = unitResource.ResourceCosts.TryGetValue(ResourceType.Food, out int fCost) ? fCost : 0;	

		if(!GameManager.Instance.CanTrain(FactionType.Player)) return false;
		if(!ResourceManager.Instance.Spend(BuildingResource.Faction, woodCost, goldCost, foodCost)) return false;

        Unit unitToSpawn = InstantiateUnit(unitResource.UnitScene, unitResource);

		Enqueue(unitToSpawn, TrainTime);
		return true;
    }

    private Unit InstantiateUnit(PackedScene unitScene, UnitResource statsToAssign)
    {
        if(unitScene == null)
        {
            GD.PushError($"Cannot spawn unit for building {Name} because the PackedScene is null.");
            return null;
        }
        var unit = unitScene.Instantiate<Unit>();
        unit.stats = statsToAssign;
        unit.stats.Faction = BuildingResource.Faction;

        var rootCell = GridManager.WorldPositionToGridCell(GlobalPosition);
        List<Vector2I> adjacentCells = GameManager.Instance.Grid.GetAdjecentCells(rootCell, BuildingResource.Dimensions, true);
        unit.GlobalPosition = adjacentCells[ (int)(GD.Randi() % adjacentCells.Count) ] * GlobalValues.CELL_SIZE; //get random position
        
        return unit;
    }

    private void SpawnUnit(Unit unit)
    {
        GetParent().AddChild(unit);
    }

    internal virtual void SetSelected(bool selected)
    {
        selectionRing.Visible = selected;
        OnSelectionChanged(selected);
    }

     protected virtual void OnSelectionChanged(bool selected)
    {

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

    #region BUILD PROGRESS VISUALS
    private void SetupBuildVisual()
    {
        foreach (var child in GetChildren().Where(node => node is Sprite2D).Cast<Sprite2D>())
        {
            if (child.Material is ShaderMaterial shader)
            {
                buildMaterial = (ShaderMaterial)shader.Duplicate();
                child.Material = buildMaterial;
                SetBuildProgress(buildProgress);
                return;
            }
        }
    }

    private void RunTestBuildAnimation()
    {
        SetBuildProgress(0f);

        if (buildTween != null && buildTween.IsValid())
        {
            buildTween.Kill();
        }

        buildTween = CreateTween();
        buildTween.TweenMethod(Callable.From<float>(ApplyBuildProgress), 0f, 1f, testBuildTime);
    }

    public void SetBuildProgress(float value, bool interpolate = false)
    {
        float target = Mathf.Clamp(value, 0f, 1f);

        if (buildTween != null && buildTween.IsValid())
        {
            buildTween.Kill();
        }

        if (!interpolate)
        {
            ApplyBuildProgress(target);
            return;
        }

        buildTween = CreateTween();
        buildTween.TweenMethod(Callable.From<float>(ApplyBuildProgress), buildProgress, target, BuildInterpolationTime);
    }

    private void ApplyBuildProgress(float value)
    {
        buildProgress = value;
        buildMaterial?.SetShaderParameter("progress", buildProgress);
    }
    #endregion
}
