using Godot;
using System;

public partial class Damageable : Node
{
    [Export] public float Health = 100f;
    [Export] public Node Target;

    public void Damage(float amount)
    {
        Health -= amount;

        if (Health > 0) return;

        GetParent().QueueFree();
    }
}
