using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	[Export] public AnimationTree AnimationTree;

	private float _speed = 20.0f;
	public float _baseJumpLift = 30f;

	private Vector3 _jumpVelocity;
	private Vector3 _movement;
	private float _attackCooldown;
	private float _attackTimer;

	private NavigationAgent3D _navigationAgent;
	private RayCast3D _jumpRaycast;
	private RayCast3D _wallRaycast;
	private Area3D _damageArea;

	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_jumpRaycast = GetNode<RayCast3D>("JumpRaycast");
		_wallRaycast = GetNode<RayCast3D>("WallRaycast");
		_damageArea = GetNode<Area3D>("DamageArea");
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
		_wallRaycast.TargetPosition = movement * 0.7f;

		bool shouldJump = !_jumpRaycast.IsColliding() || _wallRaycast.IsColliding();

		if (shouldJump && IsOnFloor())
		{
			_jumpVelocity = Vector3.Up * _baseJumpLift;
		}

		if (IsOnFloor()) _movement = movement;

		Velocity = _movement * _speed + _jumpVelocity;

		MoveAndSlide();

		if (GlobalPosition.Y < -30f) Died();

		UpdateAnimations((float)delta);

		_damageArea.GlobalPosition = GlobalPosition + movement;
	}

	public override void _Process(double delta)
	{
		_attackCooldown -= (float)delta;
		_attackTimer -= (float)delta;

		if (_attackCooldown > 0) return;

		if (GlobalPosition.DistanceTo(Player.Me.GlobalPosition) > 3f) return;

		Attack();
	}

	public void Died()
	{
		QueueFree();
	}

	private void UpdateAnimations(float delta)
	{
		Vector3 oldRotation = GlobalRotation;

		if (_movement != Vector3.Zero) LookAt(GlobalPosition + _movement);

		GlobalRotation = MathHelper.FixedLerp(oldRotation, GlobalRotation, 8f, delta);

		AnimationTree.Set("parameters/JumpBlend/blend_amount", MathHelper.FixedLerp((float)AnimationTree.Get("parameters/JumpBlend/blend_amount"), IsOnFloor() ? 0 : 1, 2f, delta));
	}

	private void Attack()
	{
		_attackCooldown = 2f;
		_attackTimer = 1f;

		AnimationTree.Set("parameters/Attack/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);

		foreach (Node3D body in _damageArea.GetOverlappingBodies())
		{
			if (!(body is Player player)) continue;

			player.GetNode<Damageable>("Damageable").Damage(20f);
		}
	}
}
