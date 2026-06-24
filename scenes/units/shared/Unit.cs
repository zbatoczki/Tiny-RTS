using Godot;
using Game.Autoload;
using Game.Component;
using Game.Resources.Unit;
using Game.FSM;
using Game.Resources.Gathering;
namespace Game.Units;

[GlobalClass]
public abstract partial class Unit : CharacterBody2D
{
    [Signal]
    public delegate void UnitDiedEventHandler(Unit unit);

    [Export] public UnitResource stats;
    [Export] public bool HasAttackDirections {get; private set;} = false;
    [Export] public bool CanAttack {get; private set;} = true;
    [Export] public bool DrawMovementPath = false;

    public Vector2 targetPosition    = Vector2.Zero;
    public Vector2[] path = [];
    public int pathIndex;
    public CollisionShape2D  collisionShape;
    public AnimatedSprite2D animatedSprite2D;
    public DamageComponent damageComponent;
    public Unit AttackTarget 
    {
        get; 
        set
        {
            field = value;
            if(value != null)
                MoveTo(value.GlobalPosition);
        }
    }
    
    
    public StateMachine stateMachine;
    public float CurrentHealth { get; private set; }
    private HealthComponent healthComponent;
    private Sprite2D selectionRing;

    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));
        collisionShape = GetNodeOrNull<CollisionShape2D>(nameof(CollisionShape2D));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        damageComponent = GetNode<DamageComponent>(nameof(DamageComponent));
        selectionRing = GetNode<Sprite2D>("SelectionRing");
        selectionRing.Visible = false;

        stateMachine = GetNode<StateMachine>(nameof(StateMachine));
        stateMachine.Init(this);

        damageComponent.Scale *= stats.AttackRange;
        damageComponent.BodyEntered += OnEnemyEntered;
        damageComponent.BodyExited += OnEnemyExit;

        CurrentHealth = stats.MaxHealth;
        healthComponent.SetMaxValue(stats.MaxHealth);
    }

    public override void _Process(double delta)
    {
        stateMachine.UpdateFrame(delta);
    }


    public override void _PhysicsProcess(double delta)
    {
        stateMachine.UpdatePhysicsFrame(delta);
        if (path.Length > 0 && AttackTarget == null && DrawMovementPath) QueueRedraw();
    }

    public override void _Draw()
    {
        if (AttackTarget != null || path.Length == 0 || pathIndex >= path.Length || !DrawMovementPath) return;

        var color = Colors.Red;
        Vector2 prev = CellCenterOffset;
        for (int i = pathIndex; i < path.Length; i++)
        {
            Vector2 local = ToLocal(path[i]);
            DrawLine(prev, local, color, 2.0f);
            DrawCircle(local, 4.0f, color);
            prev = local;
        }
    }

    public string GetCurrentState()
    {
        return stateMachine.currentState.GetType().ToString();
    }


#region POSITIONING

    /// <summary>Offset from the root node to the unit's visual center within its 64x64 cell.</summary>
    public static readonly Vector2 CellCenterOffset = new(32, 32);

    /// <summary>World position of the unit's center (the root node sits at the cell's top-left corner).</summary>
    public Vector2 CenterPosition => GlobalPosition + CellCenterOffset;

    public void FaceRight(bool faceRight)
    {
        animatedSprite2D.FlipH = faceRight;
    }

#endregion


#region SELECTION

    public virtual void SetSelected(bool selected)
    {
        selectionRing.Visible = selected;
        OnSelectionChanged(selected);
    }

    protected virtual void OnSelectionChanged(bool selected)
    {
        GD.Print($"{Name} selected={selected}");
    }

#endregion

#region Health

    public void TakeDamage(float incomingDamage)
    {
        CurrentHealth -= incomingDamage;
        healthComponent.SetCurrentHealth(CurrentHealth);
        if(CurrentHealth < 1)
            stateMachine.ForceToState<Dead>();
    }

#endregion


#region MOVEMENT

    /// <summary>
    /// Move to a world position
    /// </summary>
    /// <param name="worldTarget">A Vector2D representing a global position.</param>
    public virtual void MoveTo(Vector2 worldTarget)
    {
        targetPosition = worldTarget;
        path = GameManager.Instance?.Grid?.FindPath(CenterPosition, worldTarget) ?? [];
        pathIndex = 0;
        stateMachine.ForceToState<Move>();
    }

    /// <summary>Halts the unit: clears its path and attack target and returns it to Idle.</summary>
    public virtual void Stop()
    {
        AttackTarget = null;
        path = [];
        pathIndex = 0;
        targetPosition = Vector2.Zero;
        Velocity = Vector2.Zero;
        stateMachine.ForceToState<Idle>();
    }

#endregion


#region ATTACK STATE

    private void OnEnemyEntered(Node2D body)
    {
        if(!CanAttack) return;
        AttackTarget ??= body as Unit;
        stateMachine.ForceToState<Attack>();
    }

    private void OnEnemyExit(Node2D body)
    {
        if(body != AttackTarget) return;
        AttackTarget = null;
    }

    public virtual void Attack()
    {
        if (!IsInstanceValid(AttackTarget)) return;	
        AttackTarget.TakeDamage(stats.AttackDamage);
    }

#endregion


#region ANIMATION

    public void PlayAnimation(StringName animation)
    {
        animatedSprite2D.Play(animation);
    }

    private void OnAnimationFinsihed()
    {
        if(animatedSprite2D.Animation != "attack") return;
        if(stateMachine.currentState is Attack)
        {
            animatedSprite2D.Play("idle");
            AttackTarget.TakeDamage(1);  
        }
    }

#endregion

#region UNIT DETECTION

    protected virtual void OnUnitDetected(Unit target)
    {
		AttackTarget ??= target;
		MoveTo(target.GlobalPosition);
    }

#endregion
}
