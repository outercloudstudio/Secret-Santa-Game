using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	private float _speed = 20.0f;
	public float _baseJumpLift = 30f;

	private Vector3 _jumpVelocity;
	private Vector3 _spawnPosition;
	private Vector3 _movement;

	private NavigationAgent3D _navigationAgent;
	private RayCast3D _jumpRaycast;
	private RayCast3D _wallRaycast;

	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_jumpRaycast = GetNode<RayCast3D>("JumpRaycast");
		_wallRaycast = GetNode<RayCast3D>("WallRaycast");

		_spawnPosition = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		_navigationAgent.TargetPosition = Player.Me.GlobalPosition;

		if (_navigationAgent.IsNavigationFinished()) return;

		Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();

		if (IsOnFloor())
		{
			if (_jumpVelocity.Y < 0) _jumpVelocity.Y = 0f;
		}
		else
		{
			_jumpVelocity += GetGravity() * 6f * (float)delta;
		}

		Vector3 movement = GlobalPosition.DirectionTo(nextPathPosition);
		movement.Y = 0;
		movement = movement.Normalized();

		_jumpRaycast.GlobalPosition = GlobalPosition + movement;
		_wallRaycast.TargetPosition = movement * 0.6f;

		bool shouldJump = !_jumpRaycast.IsColliding() || _wallRaycast.IsColliding();

		if (shouldJump && IsOnFloor())
		{
			_jumpVelocity = Vector3.Up * _baseJumpLift;
		}

		if (IsOnFloor()) _movement = movement;

		Velocity = _movement * _speed + _jumpVelocity;

		MoveAndSlide();

		if (GlobalPosition.Y < -30f) GlobalPosition = _spawnPosition;
	}
}
