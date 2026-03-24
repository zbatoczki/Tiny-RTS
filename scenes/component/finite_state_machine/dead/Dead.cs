using Game.FSM;
using Godot;

public partial class Dead : State
{

	private Node2D deadScene;

    public override void _Ready()
    {
        deadScene = ResourceLoader.Load<PackedScene>("res://scenes/component/DeadComponent.tscn").Instantiate<Node2D>();
    }


	// Called when the node enters the scene tree for the first time.
	public override void Enter()
	{
		Die();
	}

	private void Die()
    {
        deadScene.GlobalPosition = unit.GlobalPosition;
        unit.Owner.AddChild(deadScene);
        unit.QueueFree();
    }
}
