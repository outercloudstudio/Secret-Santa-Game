using Godot;
using System;

public partial class Game : Node3D
{
	public static Game Me;

	[Export] public PackedScene EnemyScene;
	[Export] public Node3D SpawnPoint;
	[Export] public NavigationRegion3D NavigationRegion;

	private static bool s_Started = false;

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
		}
	}

	public static void Start()
	{
		StartRound(5f);
	}

	public static void End()
	{
		foreach (Node node in Me.GetTree().GetNodesInGroup("Enemeies"))
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

		for (int i = 0; i < difficulty; i++)
		{
			(Vector3 spawnPoint, bool success) = FindSpawnPosition();

			if (!success) continue;

			Enemy enemy = Me.EnemyScene.Instantiate<Enemy>();

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
