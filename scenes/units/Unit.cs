using Godot;
using Game.Component;
namespace Game.Units;

public partial class Unit : CharacterBody2D
{
    [Signal]
    public delegate void MovementCompletedEventHandler(Vector2I gridCellPosition);

    [Export] public float MoveSpeed = 150f;

	private HealthComponent healthComponent;

    private Vector2 targetPosition    = Vector2.Zero;
    private CollisionShape2D  collisionShape;
    private AnimatedSprite2D animatedSprite2D;
    private DamageComponent damageComponent;
    private Node2D attackTarget;
    private Timer attackTimer;
    
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

        animatedSprite2D.AnimationFinished += OnAnimationFinsihed;

        damageComponent.BodyEntered += OnEnemyEntered;
        damageComponent.BodyExited += OnEnemyExit;

        attackTimer = new();
        attackTimer.Timeout += Attack;
        AddChild(attackTimer);
    }

    public override void _PhysicsProcess(double _)
    {
        switch (currentState)
        {
            case UnitState.Moving:
                ProcessDirectMovement();
                break;
            
            case UnitState.Attacking:
                if(IsInstanceValid(attackTarget))
                    FaceRight(GlobalPosition - attackTarget.GlobalPosition > Vector2.Zero);
                break;
        }
    }

#region STATE MANAGEMENT

    private void SetState(UnitState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case UnitState.Idle:
                targetPosition = Vector2.Zero;
                Velocity = Vector2.Zero;
                attackTimer.Stop();
                animatedSprite2D.Play("idle");
                break;

            case UnitState.Moving:
                attackTimer.Stop();
                animatedSprite2D.Play("move");
                break;
            
            case UnitState.Attacking:
                targetPosition = Vector2.Zero;
                Velocity = Vector2.Zero;
                Attack();
                break;
        }
    }

#endregion

#region POSITIONING

    private void FaceRight(bool faceRight)
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


#region MOVEMENT

    public void MoveTo(Vector2 worldTarget)
    {
        targetPosition = worldTarget;
        SetState(UnitState.Moving);
    }

    private void ProcessDirectMovement()
    {
        var direction = targetPosition - GlobalPosition;

        FaceRight(direction.X < 0);

        if(direction.Length() <= 5f)
        {
            
            SetState(UnitState.Idle);
            return;
        }

        Velocity = direction.Normalized() * MoveSpeed;
        MoveAndSlide();

        
    } 

#endregion


#region ATTACK

    private void OnEnemyEntered(Node2D body)
    {
        if(attackTarget != null) return;

        attackTarget = body;
        SetState(UnitState.Attacking);
    }

    private void OnEnemyExit(Node2D body)
    {
        if(body != attackTarget) return;
        attackTarget = null;
        if(targetPosition == Vector2.Zero)
            SetState(UnitState.Idle);
        else
            SetState(UnitState.Moving);
    }

    private void Attack()
    {   
        attackTimer.Start();
        //play the attack animation
        animatedSprite2D.Play("attack");
        //send signal to damage target
        GD.Print($"{Name} unit did damage to target {attackTarget.Name}");
        //start cool down on attack before attacking again
        
    }

#endregion

#region ANIMATION

    private void OnAnimationFinsihed()
    {
        if(animatedSprite2D.Animation != "attack") return;

        if(currentState == UnitState.Attacking)
        {
            animatedSprite2D.Play("idle");
        }
    }

#endregion
}
