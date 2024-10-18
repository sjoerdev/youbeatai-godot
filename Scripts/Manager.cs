using Godot;
using System;

public partial class Manager : Node
{
	[Export] Texture2D white;
	[Export] Texture2D orange;

	[Export] PackedScene beatPrefab;
	[Export] AudioStreamPlayer2D audioPlayer;
	
	[Export] int bpm = 60;
	[Export] int beatsAmount = 32;
	
	int currentBeat = 0;
	int? previousBeat = null;
	float beatTimer = 0;

	Sprite2D[] beatSprites;
	
	public override void _Ready()
	{
		beatSprites = new Sprite2D[beatsAmount];

		// instantiate beat sprites
		for (int i = 0; i < beatsAmount; i++)
		{
			var sprite = beatPrefab.Instantiate<Sprite2D>();

			float radius = 400;
			float angle = Mathf.Pi * 2 * i / beatsAmount - Mathf.Pi / 2;
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			sprite.Position = new(x, y);

			sprite.Texture = white;
			AddChild(sprite);

			beatSprites[i] = sprite;
		}
	}

	public override void _Process(double delta)
	{
		beatTimer += (float)delta;
		if (beatTimer > 60 / bpm)
		{
			audioPlayer.Play();
			var sprite = beatSprites[currentBeat];
			sprite.Texture = orange;

			if (previousBeat != null)
			{
				var previous = beatSprites[previousBeat.Value];
				previous.Texture = white;
			}

			beatTimer = 0;
			previousBeat = currentBeat;
			currentBeat++;
			if (currentBeat == beatsAmount) currentBeat = 0;
		}
	}
}
