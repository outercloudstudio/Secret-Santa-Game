using Godot;
using System;

public partial class RangedAttack : Node3D
{
	[Export] public float Range = 20f;
	[Export] public float AttackDelay = 0.3667f;
	[Export] public float Cooldown = 2;
	[Export] public PackedScene ProjectileScene;
	[Export] public Node3D ShootOrigin;

	[Export] public AnimationTree AnimationTree;

	private Udils.ProgressableAction _attack;

	private float _attackCooldown;
	private float _attackTimer;

	private Area3D _damageArea;
	public AudioStreamPlayer3D _attackSound;

	public override void _Ready()
	{
		_attackSound = GetParent().GetNodeOrNull<AudioStreamPlayer3D>("Attack");

		_attack = new Udils.ProgressableAction()
		.Add(0f, () =>
		{
			_attackCooldown = Cooldown;
			_attackSound.Play();

			AnimationTree.Set("parameters/Attack/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		})
		.Add(AttackDelay, () =>
		{
			Node3D projectile = ProjectileScene.Instantiate<Node3D>();

			GetParent().GetParent().AddChild(projectile);

			projectile.GlobalPosition = ShootOrigin.GlobalPosition;
			projectile.LookAt(Player.Me.GlobalPosition + Vector3.Up * 0.5f);
		});
	}

	public override void _Process(double delta)
	{
		_attack.Update((float)delta);

		if (_attack.IsInProgress()) return;

		_attackCooldown -= (float)delta;

		if (_attackCooldown > 0) return;

		if (GlobalPosition.DistanceTo(Player.Me.GlobalPosition) > Range) return;

		_attack.Start();
	}
}
