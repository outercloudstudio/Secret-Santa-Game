using Godot;
using System;

public partial class Beam : MeshInstance3D
{
	public Vector3 Target;
	public Node3D Source;

	private float _timer = 0f;

	public override void _Process(double delta)
	{
		if (_timer < 0.05)
		{
			GlobalPosition = Source.GlobalPosition.Lerp(Target, 0.5f);
			LookAt(Target);
			Scale = new Vector3(0.2f, 0.2f, Source.GlobalPosition.DistanceTo(Target));
		}

		_timer += (float)delta;

		Scale = MathHelper.FixedLerp(Scale, new Vector3(0f, 0f, Scale.Z), 8f, (float)delta);

		if (Scale.X < 0.002f) QueueFree();
	}
}
