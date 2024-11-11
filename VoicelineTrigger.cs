using Godot;
using System;

public partial class VoicelineTrigger : Area3D
{
	private bool _triggered = false;

	public void OnBodyEntered(Node3D body)
	{
		if (_triggered) return;

		if (!(body is CharacterBody3D)) return;

		AudioStreamPlayer3D player = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");

		player.Finished += Game.VoicelineFinished;

		Game.QueueVoiceLine(() =>
		{
			player.Play();
		});

		_triggered = true;
	}
}
