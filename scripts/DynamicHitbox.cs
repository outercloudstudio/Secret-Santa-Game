using Godot;
using System;

public partial class DynamicHitbox : Node3D
{
	private CollisionShape3D _collisionShape;
	private MeshInstance3D _debugShape;
	private float _baseRadius;
	private float _baseHeight;

	private int _lastStage;

	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		_debugShape = _collisionShape.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

		_baseRadius = ((CapsuleShape3D)_collisionShape.Shape).Radius;
		_baseHeight = ((CapsuleShape3D)_collisionShape.Shape).Height;

		((CapsuleShape3D)_collisionShape.Shape).Radius += 0.5f;
		((CapsuleShape3D)_collisionShape.Shape).Height += 1;

		((CapsuleMesh)_debugShape.Mesh).Radius += 0.5f;
		((CapsuleMesh)_debugShape.Mesh).Height += 1f;
	}

	public override void _PhysicsProcess(double delta)
	{
		// int currentStage = (int)(GlobalPosition.DistanceTo(Player.Me.GlobalPosition) / 30f);

		// if (currentStage == _lastStage) return;

		// if (GD.Randi() % 50 != 0) return;

		// ((CapsuleShape3D)_collisionShape.Shape).Radius = _baseRadius + GlobalPosition.DistanceTo(Player.Me.GlobalPosition) / 24f / 2f;
		// ((CapsuleShape3D)_collisionShape.Shape).Height = _baseHeight + GlobalPosition.DistanceTo(Player.Me.GlobalPosition) / 24f;

		// ((CapsuleMesh)_debugShape.Mesh).Radius = ((CapsuleShape3D)_collisionShape.Shape).Radius;
		// ((CapsuleMesh)_debugShape.Mesh).Height = ((CapsuleShape3D)_collisionShape.Shape).Height;

		// _lastStage = currentStage;
	}
}
