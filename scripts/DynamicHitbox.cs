using Godot;
using System;

public partial class DynamicHitbox : Node3D
{
	private CollisionShape3D _collisionShape;
	private MeshInstance3D _debugShape;
	private float _baseRadius;
	private float _baseHeight;

	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		_debugShape = _collisionShape.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

		_baseRadius = ((CapsuleShape3D)_collisionShape.Shape).Radius;
		_baseHeight = ((CapsuleShape3D)_collisionShape.Shape).Height;
	}

	public override void _Process(double delta)
	{
		((CapsuleShape3D)_collisionShape.Shape).Radius = _baseRadius + GlobalPosition.DistanceTo(Player.Me.GlobalPosition) / 15f / 2f;
		((CapsuleShape3D)_collisionShape.Shape).Height = _baseHeight + GlobalPosition.DistanceTo(Player.Me.GlobalPosition) / 15f;

		((CapsuleMesh)_debugShape.Mesh).Radius = ((CapsuleShape3D)_collisionShape.Shape).Radius;
		((CapsuleMesh)_debugShape.Mesh).Height = ((CapsuleShape3D)_collisionShape.Shape).Height;
	}
}
