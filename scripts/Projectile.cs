using Godot;
using System;

public partial class Projectile : Area3D
{
	[Export] public float Damage = 20f;
	[Export] public float Speed = 30f;
	[Export] public float Liftime = 10f;

	private float _timer;

	public override void _PhysicsProcess(double delta)
	{
		_timer += (float)delta;

		GlobalPosition += -GlobalBasis.Z * Speed * (float)delta;

		foreach (Node3D node in GetOverlappingBodies())
		{
			if (node is Enemy) continue;

			if (node is Player player) player.GetNode<Damageable>("Damageable").Damage(Damage);

			QueueFree();
		}

		if (_timer >= Liftime) QueueFree();
	}
}
