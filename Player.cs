using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
	[Export] public Panel ScreenOverlay;
	[Export] public Node3D SpawnPosition;

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
	private float _onFloorTimer = 0f;
	private float _wallRunningTimer = 0f;
	private Vector3 _lastWallRunningNormal = Vector3.Zero;
	private bool _spawnFogActive = false;
	private float _cameraWallTilt = 0f;

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
		_onFloorTimer -= (float)delta;
		_wallRunningTimer -= (float)delta;

		if (IsOnFloor())
		{
			_onFloorTimer = 0.1f;
		}

		Vector2 movementInput = Input.GetVector("left", "right", "back", "foward");
		Vector3 movementVector = GlobalBasis.X * movementInput.X + -GlobalBasis.Z * movementInput.Y;

		bool wallRunning = false;

		var (isNextToWallRight, wallRightNormal) = IsNextToWall(GlobalBasis.X);
		var (isNextToWallLeft, wallLeftNormal) = IsNextToWall(-GlobalBasis.X);

		if (isNextToWallRight && movementVector.Dot(wallRightNormal) < 0f) wallRunning = true;
		if (isNextToWallLeft && movementVector.Dot(wallLeftNormal) < 0f) wallRunning = true;

		if (wallRunning) _wallRunningTimer = 0.3f;

		if (isNextToWallRight)
		{
			_lastWallRunningNormal = wallRightNormal;
			_cameraWallTilt = 1f;
		}

		if (isNextToWallLeft)
		{
			_lastWallRunningNormal = wallLeftNormal;
			_cameraWallTilt = -1f;
		}

		if (isNextToWallRight && isNextToWallLeft)
		{
			_lastWallRunningNormal = Vector3.Zero;
			_cameraWallTilt = 0f;
		}

		if (!wallRunning) _cameraWallTilt = 0f;

		if (IsOnFloor())
		{
			if (_jumpVelocity.Y < 0) _jumpVelocity.Y = 0f;
			_dashVelocity = new Vector3(MathHelper.FixedLerp(_dashVelocity.X, 0f, 16f, (float)delta), _dashVelocity.Y, MathHelper.FixedLerp(_dashVelocity.Z, 0f, 16f, (float)delta));
		}
		if (wallRunning)
		{
			_jumpVelocity += GetGravity() * 1f * (float)delta;

			_dashVelocity = new Vector3(MathHelper.FixedLerp(_dashVelocity.X, 0f, 1f, (float)delta), _dashVelocity.Y, MathHelper.FixedLerp(_dashVelocity.Z, 0f, 1, (float)delta));
		}
		else
		{
			_jumpVelocity += GetGravity() * 6f * (float)delta;
			_dashVelocity = new Vector3(MathHelper.FixedLerp(_dashVelocity.X, 0f, 1f, (float)delta), _dashVelocity.Y, MathHelper.FixedLerp(_dashVelocity.Z, 0f, 1f, (float)delta));
		}

		if (_spawnFogActive) movementInput = Vector2.Zero;

		if (movementInput.Length() > 0.1f)
		{
			_movementSpeed = MathHelper.FixedLerp(_movementSpeed, MaxSpeed, 2f, (float)delta);
		}
		else
		{
			_movementSpeed = MathHelper.FixedLerp(_movementSpeed, Speed, 8f, (float)delta);
		}

		if (IsOnFloor() || wallRunning)
		{
			_movementVelocity = MathHelper.FixedLerp(_movementVelocity, movementVector * _movementSpeed, 16f, (float)delta);
		}
		else
		{
			_movementVelocity = MathHelper.FixedLerp(_movementVelocity, movementVector * _movementSpeed, 4, (float)delta);
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

		if (_spawnFogActive) return;

		if (@event.IsActionPressed("jump"))
		{
			_jumpCharge = BaseJump;
			_chargingJump = true;
		}

		if (@event.IsActionReleased("jump"))
		{
			_chargingJump = false;

			if (RecentlyTouchingFloor() || (RecentlyRanWall() && (_lastWallRunningNormal == Vector3.Zero || _lastWallRunningNormal.Dot(-_camera.GlobalBasis.Z) >= 0)))
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

		_camera.Fov = MathHelper.FixedLerp(_camera.Fov, 90f + (_chargingJump ? _jumpCharge * 20f : 0f) + (_movementSpeed - Speed) / MaxSpeed * 30f + (Velocity.Y < 0 ? Math.Abs(Velocity.Y) / 8f : 0f), 8f, (float)delta);

		HandleScreenshake((float)delta);

		Color fogColor = new Color("#c09e9a");
		fogColor.A = 1f - (GlobalPosition.Y + 20f) / 20f;

		if (_spawnFogActive)
		{
			fogColor.A = 1f - (SpawnPosition.GlobalPosition.Y + 50f - GlobalPosition.Y) / 20f;

			if (fogColor.A <= 0f) _spawnFogActive = false;
		}

		ScreenOverlay.Modulate = fogColor;

		if (GlobalPosition.Y < -30f)
		{
			GlobalPosition = SpawnPosition.GlobalPosition + Vector3.Up * 60f;

			_spawnFogActive = true;

			_chargingJump = false;
		}

		_camera.Rotation = new Vector3(_camera.Rotation.X, _camera.Rotation.Y, MathHelper.FixedLerp(_camera.Rotation.Z, Mathf.DegToRad(_cameraWallTilt * 10f), 8f, (float)delta));
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

	private (bool, Vector3) IsNextToWall(Vector3 direction)
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(GlobalPosition + Vector3.Down * 0.705f, GlobalPosition + Vector3.Down * 0.705f + direction);
		Dictionary hit = spaceState.IntersectRay(query);

		if (hit.Count == 0) return (false, Vector3.Zero);

		return (true, (Vector3)hit["normal"]);
	}

	private bool RecentlyTouchingFloor()
	{
		return _onFloorTimer > 0f;
	}

	private bool RecentlyRanWall()
	{
		return _wallRunningTimer > 0f;
	}

}
