using Godot;

public static class MathHelper
{
    public static float FixedLerp(float a, float b, float decay, float delta)
    {
        return b + (a - b) * Mathf.Exp(-decay * delta);
    }

    public static Vector2 FixedLerp(Vector2 a, Vector2 b, float decay, float delta)
    {
        return b + (a - b) * Mathf.Exp(-decay * delta);
    }

    public static Vector3 FixedLerp(Vector3 a, Vector3 b, float decay, float delta)
    {
        return b + (a - b) * Mathf.Exp(-decay * delta);
    }

    public static Color FixedLerp(Color a, Color b, float decay, float delta)
    {
        return b + (a - b) * Mathf.Exp(-decay * delta);
    }
}