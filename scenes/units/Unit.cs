using Godot;
using Game.Component;
using Game.Resources.Unit;
using Game.FSM;
namespace Game.Units;

public abstract partial class Unit : CharacterBody2D
{
    [Export] public UnitStats stats;

    public Vector2 targetPosition    = Vector2.Zero;
    public CollisionShape2D  collisionShape;
    public AnimatedSprite2D animatedSprite2D;
    public DamageComponent damageComponent;
    public Unit attackTarget;
    
    private StateMachine stateMachine;
    private HealthComponent healthComponent;

    enum UnitState
    {
        Idle,
        Moving,
        Attacking,
        Dead
    }
    private UnitState currentState = UnitState.Idle;


    public override void _Ready()
    {
        healthComponent = GetNode<HealthComponent>(nameof(HealthComponent));
        collisionShape = GetNodeOrNull<CollisionShape2D>(nameof(CollisionShape2D));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        damageComponent = GetNode<DamageComponent>(nameof(DamageComponent));

        stateMachine = GetNode<StateMachine>(nameof(StateMachine));
        stateMachine.Init(this);

        animatedSprite2D.AnimationFinished += OnAnimationFinsihed;

        damageComponent.BodyEntered += OnEnemyEntered;
        damageComponent.BodyExited += OnEnemyExit;

        
    }

    public override void _Process(double delta)
    {
        stateMachine.UpdateFrame(delta);
    }


    public override void _PhysicsProcess(double delta)
    {
        stateMachine.UpdatePhysicsFrame(delta);
    }


#region POSITIONING

    public void FaceRight(bool faceRight)
    {
        animatedSprite2D.FlipH = faceRight;
        damageComponent.FlipH(faceRight);
    }

#endregion


#region SELECTION

    public virtual void SetSelected(bool selected)
    {
        healthComponent.Visible = selected;
        OnSelectionChanged(selected);
    }

    protected virtual void OnSelectionChanged(bool selected)
    {
        GD.Print($"{Name} selected={selected}");
    }

#endregion

#region Health

    public void TakeDamage(int incomingDamage)
    {
        stats.Health -= incomingDamage;
        if(stats.Health < 1)
            stateMachine.ForceToState<Dead>();
    }

#endregion


#region MOVEMENT

    public void MoveTo(Vector2 worldTarget)
    {
        targetPosition = worldTarget;
        stateMachine.ForceToState<Move>();
    }

#endregion


#region ATTACK STATE

    private void OnEnemyEntered(Node2D body)
    {
        if(attackTarget != null) return;

        attackTarget = body as Unit;
        GD.Print($"{Name} targeting {attackTarget.Name}");
        stateMachine.ForceToState<Attack>();
    }

    private void OnEnemyExit(Node2D body)
    {
        if(body != attackTarget) return;
        attackTarget = null;
        if(targetPosition == Vector2.Zero)
            stateMachine.ForceToState<Idle>();
        else
            stateMachine.ForceToState<Move>();
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
            attackTarget.TakeDamage(1);  
        }
    }

#endregion
}
