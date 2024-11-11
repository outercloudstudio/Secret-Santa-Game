using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 50f;


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
		Velocity += GetGravity() * 6f * (float)delta;

		if (IsOnFloor())
		{
			Velocity = new Vector3(MathHelper.FixedLerp(Velocity.X, 0f, 16f, (float)delta), Velocity.Y, MathHelper.FixedLerp(Velocity.Z, 0f, 16f, (float)delta));
		}
		else
		{
			Velocity = new Vector3(MathHelper.FixedLerp(Velocity.X, 0f, 1f, (float)delta), Velocity.Y, MathHelper.FixedLerp(Velocity.Z, 0f, 1f, (float)delta));
		}

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
			_jumpCharge = 0f;
			_chargingJump = true;
		}

		if (@event.IsActionReleased("jump"))
		{
			_chargingJump = false;

			if (IsOnFloor())
			{
				Velocity += -_camera.GlobalBasis.Z * JumpVelocity * _jumpCharge;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (_chargingJump)
		{
			_jumpCharge += (float)delta;

			if (_jumpCharge > 1f) _jumpCharge = 1f;

			_camera.Fov = MathHelper.FixedLerp(_camera.Fov, 90f + _jumpCharge * 20f, 8f, (float)delta);

			_shakeIntensity = _jumpCharge * 0.01f;
		}
		else
		{
			_camera.Fov = MathHelper.FixedLerp(_camera.Fov, 90f, 8f, (float)delta);
		}

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
