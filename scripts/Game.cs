using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node3D
{
	public static Game Me;

	[Export] public PackedScene EnemyNormalScene;
	[Export] public PackedScene EnemyLargeScene;
	[Export] public PackedScene EnemyRangedScene;
	[Export] public Node3D SpawnPoint;
	[Export] public NavigationRegion3D NavigationRegion;

	private static bool s_Started = false;
	private static float s_Difficulty = 5f;


	public override void _Ready()
	{
		Me = this;
	}

	public override void _Process(double delta)
	{
		if (!s_Started)
		{
			s_Started = true;

			Start();

			return;
		}

		if (GetTree().GetNodesInGroup("Enemies").Count > 0) return;


		s_Difficulty += 5f;
		StartRound(s_Difficulty);
	}

	public static void Start()
	{
		s_Difficulty = 5f;

		StartRound(s_Difficulty);
	}

	public static void End()
	{
		foreach (Node node in Me.GetTree().GetNodesInGroup("Enemies"))
		{
			node.QueueFree();
		}
	}

	public static void Restart()
	{
		End();
		Start();
	}

	public static void StartRound(float difficulty)
	{
		Me.NavigationRegion.BakeNavigationMesh(false);

		for (float usedScore = 0; usedScore < difficulty; usedScore++)
		{
			(Vector3 spawnPoint, bool success) = FindSpawnPosition();

			if (!success) continue;

			List<Udils.WeightedRandom<PackedScene>.Element> enemies = new List<Udils.WeightedRandom<PackedScene>.Element>() {
				new Udils.WeightedRandom<PackedScene>.Element {
					Weight = 1f / 1f,
					Value = Me.EnemyNormalScene
				}
			};

			if (difficulty - usedScore >= 2f)
			{
				enemies.Add(new Udils.WeightedRandom<PackedScene>.Element
				{
					Weight = 1f / 2f,
					Value = Me.EnemyLargeScene
				});

				enemies.Add(new Udils.WeightedRandom<PackedScene>.Element
				{
					Weight = 1f / 2f,
					Value = Me.EnemyRangedScene
				});
			}

			Udils.WeightedRandom<PackedScene>.Element choice = new Udils.WeightedRandom<PackedScene>(enemies).GetElement();

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
			Vector3 spawnPosition = NavigationServer3D.Singleton.RegionGetRandomPoint(Me.NavigationRegion.GetRid(), Me.NavigationRegion.NavigationLayers, true);

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
}
