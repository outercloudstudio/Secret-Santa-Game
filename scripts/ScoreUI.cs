using Godot;
using System;

public partial class ScoreUI : CanvasLayer
{
    public static ScoreUI Me;

    private float _animatedScore;

    public override void _Ready()
    {
        Me = this;
    }

    public override void _Process(double delta)
    {
        _animatedScore = MathHelper.FixedLerp(_animatedScore, Game.InProgress ? Game.Score : Game.Highscore, 8f, (float)delta);

        if (Mathf.Abs(_animatedScore - Game.Score) < 0.5f) _animatedScore = Game.Score;

        Game.Me.ScoreLabel.Text = MathF.Floor(_animatedScore).ToString();
    }
}
