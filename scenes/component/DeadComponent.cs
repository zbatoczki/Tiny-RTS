using Godot;
namespace Game.Component;
public partial class DeadComponent : Node2D
{
	[Export]
	private float decomposeDelayInSeconds = 5.0f;
	private AnimatedSprite2D sprite;

	public override async void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		await ToSignal(GetTree().CreateTimer(decomposeDelayInSeconds), SceneTreeTimer.SignalName.Timeout);

		sprite.Play("decompose");

		await ToSignal(sprite, AnimatedSprite2D.SignalName.AnimationFinished);

		QueueFree();
	}
}
