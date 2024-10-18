using Godot;
using System;

public partial class Manager : Node
{
	[Export] AudioStreamPlayer2D audioPlayer;
	[Export] int bpm = 60;
	
	int currentBeat = 0;
	float beatTimer = 0;
	
	public override void _Ready()
	{
		// start
	}

	public override void _Process(double delta)
	{
		beatTimer += (float)delta;
		
		if (beatTimer > 60 / bpm)
		{
			audioPlayer.Play();
			beatTimer = 0;
		}
	}
}
