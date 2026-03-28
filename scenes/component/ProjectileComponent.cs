using Game.Units;
using Godot;

public partial class ProjectileComponent : Area2D
{
	[Export] public float TravelTime = 0.5f;
	[Export] public float ArcHeight = 80f;

	private Unit attackTarget;
	private Vector2 startPosition;
	private Vector2 endPosition;
	private float elapsedTime = 0f;
	private bool hasHit = false;
	private float damage;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(hasHit) return;

		elapsedTime += (float)delta;
		float t = Mathf.Clamp(elapsedTime / TravelTime, 0f, 1f);

		Vector2 previousPosition = GlobalPosition;
		Vector2 linearPosition = startPosition.Lerp(endPosition, t);

		float arcOffset = -ArcHeight * Mathf.Sin(t * Mathf.Pi);
		GlobalPosition = linearPosition + new Vector2(0f, arcOffset);

		Vector2 travelDirection = GlobalPosition - previousPosition;
		if(travelDirection != Vector2.Zero)
		{
			Rotation = travelDirection.Angle();
		}

		if(t >= 1f)
		{
			QueueFree();
		}
	}

	public void Launch(Vector2 from, float damage, Unit target)
	{
		startPosition = from;
		endPosition = target.GlobalPosition;
		this.damage = damage;
		elapsedTime = 0f;

		GlobalPosition = from;
		attackTarget = target;

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		Unit unitBody = body as Unit;
		if(hasHit || unitBody != attackTarget) return;

		hasHit = true;
		unitBody.TakeDamage(damage);

		CallDeferred(Node.MethodName.QueueFree);
	}
}
