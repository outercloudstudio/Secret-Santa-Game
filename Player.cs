using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public float Speed = 10.0f;
	public float MaxSpeed = 30.0f;
	public float BaseJump = 0.5f;
	public float JumpVelocity = 50f;

	private Vector3 _dashVelocity;
	private Vector3 _movementVelocity;
	private Vector3 _jumpVelocity;
	private float _movementSpeed;

	private float _cameraSensitivity = 0.008f;
	private Camera3D _camera;
	private RayCast3D _floorRaycast;
	private bool _chargingJump = false;
	private float _jumpCharge = 0f;

	private float _shakeTimer = 0f;
	private Vector3 _shakeDirection = Vector3.Right;
	private Vector3 _shakePosition;
	private float _shakeIntensity = 0f;
	private RandomNumberGenerator _random = new RandomNumberGenerator();

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Camera3D");
		_floorRaycast = GetNode<RayCast3D>("FloorRaycast");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsOnFloor())
		{
			if (_jumpVelocity.Y < 0) _jumpVelocity.Y = 0f;
			_dashVelocity = new Vector3(MathHelper.FixedLerp(_dashVelocity.X, 0f, 16f, (float)delta), _dashVelocity.Y, MathHelper.FixedLerp(_dashVelocity.Z, 0f, 16f, (float)delta));
		}
		else
		{
			_jumpVelocity += GetGravity() * 6f * (float)delta;
			_dashVelocity = new Vector3(MathHelper.FixedLerp(_dashVelocity.X, 0f, 1f, (float)delta), _dashVelocity.Y, MathHelper.FixedLerp(_dashVelocity.Z, 0f, 1f, (float)delta));
		}

		Vector2 movementInput = Input.GetVector("left", "right", "back", "foward");

		if (movementInput.Length() > 0.1f)
		{
			_movementSpeed = MathHelper.FixedLerp(_movementSpeed, MaxSpeed, 2f, (float)delta);
		}
		else
		{
			_movementSpeed = MathHelper.FixedLerp(_movementSpeed, Speed, 8f, (float)delta);
		}

		if (IsOnFloor())
		{
			_movementVelocity = MathHelper.FixedLerp(_movementVelocity, (GlobalBasis.X * movementInput.X + -GlobalBasis.Z * movementInput.Y) * _movementSpeed, 16f, (float)delta);
		}
		else
		{
			_movementVelocity = MathHelper.FixedLerp(_movementVelocity, (GlobalBasis.X * movementInput.X + -GlobalBasis.Z * movementInput.Y) * _movementSpeed, 4, (float)delta);
		}

		Velocity = _dashVelocity + _movementVelocity + _jumpVelocity;

		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			RotateY(-mouseMotionEvent.Relative.X * _cameraSensitivity);
			_camera.GlobalRotation = new Vector3(_camera.GlobalRotation.X, GlobalRotation.Y, _camera.GlobalRotation.Z);
			_camera.Rotation += new Vector3(-mouseMotionEvent.Relative.Y * _cameraSensitivity, 0, 0);

			if (_camera.Rotation.X > Mathf.DegToRad(80f)) _camera.Rotation = new Vector3(Mathf.DegToRad(80f), _camera.Rotation.Y, _camera.Rotation.Z);
			if (_camera.Rotation.X < -Mathf.DegToRad(80f)) _camera.Rotation = new Vector3(-Mathf.DegToRad(80f), _camera.Rotation.Y, _camera.Rotation.Z);
		}

		if (@event.IsActionPressed("jump"))
		{
			_jumpCharge = BaseJump;
			_chargingJump = true;
		}

		if (@event.IsActionReleased("jump"))
		{
			_chargingJump = false;

			if (IsOnFloor())
			{
				Vector3 dashImpulse = -_camera.GlobalBasis.Z * JumpVelocity * _jumpCharge;

				_dashVelocity = new Vector3(dashImpulse.X, 0, dashImpulse.Z);
				_jumpVelocity = new Vector3(0, dashImpulse.Y, 0);
			}
		}
	}

	public override void _Process(double delta)
	{
		if (_chargingJump)
		{
			_jumpCharge += (float)delta;

			if (_jumpCharge > 0.8f) _jumpCharge = 0.8f;

			_shakeIntensity = _jumpCharge * 0.01f;
		}

		_camera.Fov = MathHelper.FixedLerp(_camera.Fov, 90f + (_chargingJump ? _jumpCharge * 20f : 0f) + (_movementSpeed - Speed) / MaxSpeed * 30f, 8f, (float)delta);

		HandleScreenshake((float)delta);
	}

	private void HandleScreenshake(float delta)
	{
		_shakeTimer -= delta;

		if (_shakeTimer <= 0)
		{
			_shakeTimer = _random.RandfRange(0.02f, 0.03f);

			_shakeDirection = _camera.Basis.X.Rotated(-_camera.Basis.Z.Normalized(), _random.RandfRange(0f, Mathf.Pi * 2f));
		}

		_shakePosition = _shakeDirection * Mathf.Pow(_shakeIntensity * 3f, 0.8f);

		_shakeIntensity -= delta * 8f;
		if (_shakeIntensity < 0f) _shakeIntensity = 0f;

		_camera.Position = new Vector3(0f, 0.593f, 0f) + _shakePosition;
	}
}
