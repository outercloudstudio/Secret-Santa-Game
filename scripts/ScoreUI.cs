using Godot;
using System;

public partial class ScoreUI : CanvasLayer
{
    public static ScoreUI Me;

    [Export] public PackedScene BonusScene;
    [Export] public Control BonusParent;

    private float _animatedScore;

    public override void _Ready()
    {
        Me = this;
    }

    public override void _Process(double delta)
    {
        _animatedScore = MathHelper.FixedLerp(_animatedScore, Game.InProgress ? Game.Score : Game.Highscore, 8f, (float)delta);

        if (Mathf.Abs(_animatedScore - (Game.InProgress ? Game.Score : Game.Highscore)) < 0.5f) _animatedScore = Game.Score;

        Game.Me.ScoreLabel.Text = MathF.Floor(_animatedScore).ToString();
    }

    public static void AddScore(string message)
    {
        Vector2 viewportSize = Me.GetViewport().GetVisibleRect().Size;

        Control bonus = Me.BonusScene.Instantiate<Control>();
        bonus.GetNode<Label>("Label").Text = message;

        Me.BonusParent.AddChild(bonus);

        bonus.Position = viewportSize / 2f - bonus.Size / 2f + Vector2.Down * 25f * Me.GetTree().GetNodeCountInGroup("Bonus");
    }
}
