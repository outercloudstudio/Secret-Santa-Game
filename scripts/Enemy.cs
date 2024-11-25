using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	[Export] public float Speed = 20f;
	[Export] public float WallRaycastLength = 0.7f;
	[Export] public float TargetPlayerDistance = 0f;
	[Export] public float TargetPlayerDistanceBuffer = 0f;
	[Export] public AnimationTree AnimationTree;

	public float _baseJumpLift = 30f;

	private Vector3 _jumpVelocity;
	private Vector3 _movement;
	private bool _inRange;
	private float _timerTillPathRecalculation;
	private Vector3 _nextPathPosition;

	private NavigationAgent3D _navigationAgent;
	private RayCast3D _jumpRaycast;
	private RayCast3D _wallRaycast;
	private Area3D _damageArea;

	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_jumpRaycast = GetNode<RayCast3D>("JumpRaycast");
		_wallRaycast = GetNode<RayCast3D>("WallRaycast");
		_damageArea = GetNodeOrNull<Area3D>("DamageArea");
	}

	public override void _PhysicsProcess(double delta)
	{
		_timerTillPathRecalculation -= (float)delta;

		_navigationAgent.TargetPosition = Player.Me.GlobalPosition;

		if (_navigationAgent.IsNavigationFinished()) return;

		if (!_inRange)
		{
			_inRange = Player.Me.GlobalPosition.DistanceTo(GlobalPosition) <= TargetPlayerDistance;
		}
		else
		{
			_inRange = Player.Me.GlobalPosition.DistanceTo(GlobalPosition) <= TargetPlayerDistance + TargetPlayerDistanceBuffer;
		}

		if (_timerTillPathRecalculation <= 0)
		{
			_nextPathPosition = _navigationAgent.GetNextPathPosition();

			_timerTillPathRecalculation = GD.Randf() * 2f + 0.2f;
		}

		if (IsOnFloor())
		{
			if (_jumpVelocity.Y < 0) _jumpVelocity.Y = 0f;
		}
		else
		{
			_jumpVelocity += GetGravity() * 6f * (float)delta;
		}

		Vector3 movement = GlobalPosition.DirectionTo(_nextPathPosition);
		movement.Y = 0;
		movement = movement.Normalized();

		_jumpRaycast.GlobalPosition = GlobalPosition + movement;
		_wallRaycast.TargetPosition = movement * WallRaycastLength;

		bool shouldJump = !_jumpRaycast.IsColliding() || _wallRaycast.IsColliding();

		if (shouldJump && IsOnFloor())
		{
			_jumpVelocity = Vector3.Up * _baseJumpLift;
		}

		if (IsOnFloor()) _movement = movement;

		Velocity = (_inRange ? Vector3.Zero : _movement * Speed) + _jumpVelocity;

		MoveAndSlide();

		if (GlobalPosition.Y < -30f) Died();

		UpdateAnimations((float)delta);

		if (_damageArea != null) _damageArea.GlobalPosition = GlobalPosition + movement;
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
		AnimationTree.Set("parameters/MoveBlend/blend_amount", MathHelper.FixedLerp((float)AnimationTree.Get("parameters/MoveBlend/blend_amount"), Velocity.Length() > 0.1f ? 1f : 0f, 8f, delta));
	}
}
