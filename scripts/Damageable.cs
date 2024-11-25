using Godot;
using System;

public partial class Damageable : Node
{
    [Signal] public delegate void KilledEventHandler();
    [Signal] public delegate void HurtEventHandler();

    [Export] public float Health = 100f;

    public void Damage(float amount)
    {
        EmitSignal(SignalName.Hurt);

        Health -= amount;

        if (Health > 0) return;

        EmitSignal(SignalName.Killed);
    }
}
