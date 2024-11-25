using Godot;
using System;

public partial class Projectile : Area3D
{
	[Export] public float Damage = 20f;
	[Export] public float Speed = 30f;

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += -GlobalBasis.Z * Speed * (float)delta;

		foreach (Node3D node in GetOverlappingBodies())
		{
			GD.Print(node.Name);

			if (node is Enemy) continue;

			if (node is Player player) player.GetNode<Damageable>("Damageable").Damage(Damage);

			QueueFree();
		}
	}
}
