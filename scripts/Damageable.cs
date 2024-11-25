using Godot;
using System;

public partial class Damageable : Node
{
    [Signal] public delegate void KilledEventHandler();

    [Export] public float Health = 100f;

    public void Damage(float amount)
    {
        Health -= amount;

        if (Health > 0) return;

        EmitSignal(SignalName.Killed);
    }
}
