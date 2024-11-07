using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	private float _cameraSensitivity = 0.008f;
	private float _hammeraSensitivity = 0.027f;
	private Vector3 _hammerOffset = new Vector3(0f, -0f, 0f);
	private Camera3D _camera;
	private Node3D _hammer;
	private Node3D _point;

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Camera3D");
		_hammer = GetNode<Node3D>("Hammer");
		_point = GetNode<Node3D>("Point");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor()) velocity += GetGravity() * (float)delta;


		if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpVelocity;


		Vector2 inputDir = Input.GetVector("left", "right", "foward", "back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;

		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			RotateY(-mouseMotionEvent.Relative.X * _cameraSensitivity);
			_camera.RotateX(-mouseMotionEvent.Relative.Y * _cameraSensitivity);

			_hammerOffset += new Vector3(mouseMotionEvent.Relative.X, -mouseMotionEvent.Relative.Y, 0) * _hammeraSensitivity;

			_hammerOffset.X = Mathf.Clamp(_hammerOffset.X, -1, 1);
			_hammerOffset.Y = Mathf.Clamp(_hammerOffset.Y, -1, 1);
		}
	}

	public override void _Process(double delta)
	{
		_hammer.Position = new Vector3(0, -0.322f, -0.916f) + _hammerOffset;
		_hammer.LookAt(ToGlobal(_hammer.Position + new Vector3(0, -0.322f, -0.916f) + _hammerOffset * 2));

		_point.GlobalPosition = ToGlobal(_hammer.Position + new Vector3(0, -0.322f, -0.916f) + _hammerOffset * 2);
	}
}
