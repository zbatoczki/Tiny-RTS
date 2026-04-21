using Godot;
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

    [Export] public UnitStats stats;

    public Vector2 targetPosition    = Vector2.Zero;
    public CollisionShape2D  collisionShape;
    public AnimatedSprite2D animatedSprite2D;
    public DamageComponent damageComponent;
    public Unit AttackTarget {get; set;}
    
    
    public StateMachine stateMachine;
    private HealthComponent healthComponent;
    private float currentHealth;
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

        currentHealth = stats.MaxHealth;
        healthComponent.SetMaxValue(stats.MaxHealth);
    }

    public override void _Process(double delta)
    {
        stateMachine.UpdateFrame(delta);
    }


    public override void _PhysicsProcess(double delta)
    {
        stateMachine.UpdatePhysicsFrame(delta);
    }

    public string GetCurrentState()
    {
        return stateMachine.currentState.GetType().ToString();
    }


#region POSITIONING

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
        currentHealth -= incomingDamage;
        healthComponent.SetCurrentHealth(currentHealth);
        if(currentHealth < 1)
            stateMachine.ForceToState<Dead>();
    }

#endregion


#region MOVEMENT

    public virtual void MoveTo(Vector2 worldTarget)
    {
        targetPosition = worldTarget;
        stateMachine.ForceToState<Move>();
    }

#endregion


#region ATTACK STATE

    private void OnEnemyEntered(Node2D body)
    {
        if(AttackTarget == null)
        {
            AttackTarget = body as Unit;
        }
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
