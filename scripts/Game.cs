using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class Game : Node3D
{
    public static Game Me;
    public static bool InProgress = false;

    [Export] public PackedScene EnemyNormalScene;
    [Export] public PackedScene EnemyLargeScene;
    [Export] public PackedScene EnemyRangedScene;
    [Export] public PackedScene EnemyRangedBurstScene;
    [Export] public PackedScene EnemyRangedBurst2Scene;
    [Export] public PackedScene EnemyRangedSpreadScene;
    [Export] public PackedScene EnemyRangedSpread2Scene;
    [Export] public PackedScene EnemyRangedBurstSpreadScene;
    [Export] public PackedScene MenuScene;
    [Export] public Node3D SpawnPoint;
    [Export] public NavigationRegion3D NavigationRegion;
    [Export] public Label ScoreLabel;
    [Export] public Label BestLabel;
    [Export] public AnimationPlayer MenuAnimationPlayer;

    public static int Score = 0;
    public static int Highscore = 0;

    private static bool s_Started = false;
    private static float s_Difficulty = 5f;



    public override void _Ready()
    {
        Me = this;

        NavigationRegion.BakeNavigationMesh(false);

        string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string gamePath = Path.Combine(appDataPath, "Polydash");

        if (!Directory.Exists(gamePath)) Directory.CreateDirectory(gamePath);

        string highScorePath = Path.Combine(gamePath, "highscore");

        if (File.Exists(highScorePath))
        {
            string scoreText = File.ReadAllText(highScorePath);
            if (int.TryParse(scoreText, out int readScore)) Highscore = readScore;
        }
    }

    public override void _Process(double delta)
    {
        // if (!s_Started)
        // {
        //     s_Started = true;

        //     Start();

        //     return;
        // }

        if (!InProgress) return;

        if (GetTree().GetNodesInGroup("Enemies").Count > 0) return;


        s_Difficulty += 5f;
        StartRound(s_Difficulty);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("jump") && !InProgress) Start();
    }

    public static void Start()
    {
        InProgress = true;

        Me.BestLabel.Visible = false;

        s_Difficulty = 5f;
        Score = 0;

        Me.MenuAnimationPlayer.Play("out");

        StartRound(s_Difficulty);
    }

    public static void End()
    {
        InProgress = false;

        foreach (Node node in Me.GetTree().GetNodesInGroup("Enemies"))
        {
            node.QueueFree();
        }

        Me.BestLabel.Visible = true;

        if (Score > Highscore)
        {
            Highscore = Score;

            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string gamePath = Path.Combine(appDataPath, "Polydash");

            if (!Directory.Exists(gamePath)) Directory.CreateDirectory(gamePath);

            string highScorePath = Path.Combine(gamePath, "highscore");

            File.WriteAllText(highScorePath, Highscore.ToString());
        }

        Me.MenuAnimationPlayer.PlayBackwards("out");
    }

    public static void Restart()
    {
        End();
        Start();
    }

    public static void StartRound(float difficulty)
    {
        Udils.WeightedRandom<PackedScene> weightedRandom = new Udils.WeightedRandom<PackedScene>(new List<Udils.WeightedRandom<PackedScene>.Element>() {
            new Udils.WeightedRandom<PackedScene>.Element {
                Weight = 1f / 1f,
                Value = Me.EnemyNormalScene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 2f,
                Value = Me.EnemyLargeScene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 2f,
                Value = Me.EnemyRangedScene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 6f,
                Value = Me.EnemyRangedBurstScene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 6f,
                Value = Me.EnemyRangedSpreadScene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 12f,
                Value = Me.EnemyRangedBurst2Scene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 12f,
                Value = Me.EnemyRangedSpread2Scene
            },
            new Udils.WeightedRandom<PackedScene>.Element
            {
                Weight = 1f / 12f,
                Value = Me.EnemyRangedBurstSpreadScene
            }
        });

        for (float usedScore = 0; usedScore < difficulty; usedScore++)
        {
            (Vector3 spawnPoint, bool success) = FindSpawnPosition();

            if (!success) continue;

            Udils.WeightedRandom<PackedScene>.Element choice = weightedRandom.GetElement();

            if (difficulty - usedScore < 1f / choice.Weight) continue;

            usedScore--;
            usedScore += 1f / choice.Weight;

            Enemy enemy = choice.Value.Instantiate<Enemy>();

            Me.AddChild(enemy);

            enemy.GlobalPosition = spawnPoint;
        }
    }

    public static (Vector3, bool) FindSpawnPosition()
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            Vector3 spawnPosition = NavigationServer3D.Singleton.RegionGetRandomPoint(Me.NavigationRegion.GetRid(), Me.NavigationRegion.NavigationLayers, false);

            Vector3[] path = NavigationServer3D.Singleton.MapGetPath(Me.NavigationRegion.GetNavigationMap(), spawnPosition, Me.SpawnPoint.GlobalPosition, false);

            if (path[path.Length - 1].DistanceTo(Me.SpawnPoint.GlobalPosition) > 1.5f) continue;

            PhysicsDirectSpaceState3D spaceState = Me.GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(spawnPosition, spawnPosition + Vector3.Down * 5f);
            query.CollideWithAreas = true;
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);

            if (result.Count == 0) continue;

            return (spawnPosition, true);
        }

        return (Vector3.Zero, false);
    }

    public static void UpdateScore(int amount)
    {
        Score += amount;
    }
}
