using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	private float _speed = 20.0f;

	private NavigationAgent3D _navigationAgent;

	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		_navigationAgent.TargetPosition = Player.Me.GlobalPosition;

		if (_navigationAgent.IsNavigationFinished()) return;

		Velocity = GlobalPosition.DirectionTo(_navigationAgent.GetNextPathPosition()) * _speed;

		MoveAndSlide();
	}
}
