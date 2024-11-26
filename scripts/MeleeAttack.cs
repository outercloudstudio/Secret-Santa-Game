using Godot;
using System;

public partial class MeleeAttack : Node3D
{
    [Export] public float AttackDelay = 0.3667f;
    [Export] public float Damage = 20f;

    [Export] public AnimationTree AnimationTree;

    private Udils.ProgressableAction _attack;

    private float _attackCooldown;
    private float _attackTimer;

    private Area3D _damageArea;
    public AudioStreamPlayer3D _attackSound;

    public override void _Ready()
    {
        _damageArea = GetParent().GetNode<Area3D>("DamageArea");
        _attackSound = GetParent().GetNodeOrNull<AudioStreamPlayer3D>("Attack");

        _attack = new Udils.ProgressableAction()
        .Add(0f, () =>
        {
            _attackCooldown = 1f;
            _attackSound.Play();

            AnimationTree.Set("parameters/Attack/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        })
        .Add(AttackDelay, () =>
        {
            foreach (Node3D body in _damageArea.GetOverlappingBodies())
            {
                if (!(body is Player player)) continue;

                player.GetNode<Damageable>("Damageable").Damage(Damage);
            }
        });
    }

    public override void _Process(double delta)
    {
        _attack.Update((float)delta);

        if (_attack.IsInProgress()) return;

        _attackCooldown -= (float)delta;

        if (_attackCooldown > 0) return;

        if (GlobalPosition.DistanceTo(Player.Me.GlobalPosition) > 3f) return;

        _attack.Start();
    }
}
