using Godot;
using System;
using System.Linq;

public partial class Manager : Node
{
	// assets
	[Export] Texture2D active;
	[Export] Texture2D inactive;
	[Export] Texture2D current;

	[Export] AudioStreamPlayer2D firstAudioPlayer;
	[Export] AudioStreamPlayer2D secondAudioPlayer;
	[Export] AudioStreamPlayer2D thirdAudioPlayer;
	[Export] AudioStreamPlayer2D fourthAudioPlayer;

	
	// timing
	[Export] int bpm = 120;
	[Export] int beatsAmount = 16;
	int currentBeat = 0;
	int? previousBeat = null;
	float beatTimer = 0;

	Sprite2D[] firstRingSprites;
	Sprite2D[] secondRingSprites;
	Sprite2D[] thirdRingSprites;
	Sprite2D[] fourthRingSprites;

	bool[] firstRing;
	bool[] secondRing;
	bool[] thirdRing;
	bool[] fourthRing;
	
	public override void _Ready()
	{
		// setup beatplaces
		firstRing = new bool[beatsAmount];
		secondRing = new bool[beatsAmount];
		thirdRing = new bool[beatsAmount];
		fourthRing = new bool[beatsAmount];

		float chance = 0.4f;
		var random = new Random();
		for (int i = 0; i < beatsAmount; i++) firstRing[i] = random.NextSingle() < chance;
		for (int i = 0; i < beatsAmount; i++) secondRing[i] = random.NextSingle() < chance;
		for (int i = 0; i < beatsAmount; i++) thirdRing[i] = random.NextSingle() < chance;
		for (int i = 0; i < beatsAmount; i++) fourthRing[i] = random.NextSingle() < chance;

		// instantiate sprites
		firstRingSprites = new Sprite2D[beatsAmount];
		secondRingSprites = new Sprite2D[beatsAmount];
		thirdRingSprites = new Sprite2D[beatsAmount];
		fourthRingSprites = new Sprite2D[beatsAmount];
		for (int i = 0; i < beatsAmount; i++)
		{
			var firstSprite = new Sprite2D();
			var secondSprite = new Sprite2D();
			var thirdSprite = new Sprite2D();
			var fourthSprite = new Sprite2D();

			float angle = Mathf.Pi * 2 * i / beatsAmount - Mathf.Pi / 2;
			firstSprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 600;
			secondSprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 500;
			thirdSprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 400;
			fourthSprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 300;

			firstSprite.Texture = inactive;
			secondSprite.Texture = inactive;
			thirdSprite.Texture = inactive;
			fourthSprite.Texture = inactive;

			AddChild(firstSprite);
			AddChild(secondSprite);
			AddChild(thirdSprite);
			AddChild(fourthSprite);

			firstRingSprites[i] = firstSprite;
			secondRingSprites[i] = secondSprite;
			thirdRingSprites[i] = thirdSprite;
			fourthRingSprites[i] = fourthSprite;
		}
	}

	public override void _Process(double delta)
	{
		// keep time
		beatTimer += (float)delta;
		if (beatTimer > (60f / bpm) / 4)
		{
			beatTimer = 0;
			previousBeat = currentBeat;
			currentBeat++;
			if (currentBeat == beatsAmount) currentBeat = 0;
			OnBeat();
		}

		// update sprites
		for (int i = 0; i < beatsAmount; i++)
		{
			firstRingSprites[i].Texture = inactive;
			secondRingSprites[i].Texture = inactive;
			thirdRingSprites[i].Texture = inactive;
			fourthRingSprites[i].Texture = inactive;

			if (firstRing[i]) firstRingSprites[i].Texture = active;
			if (secondRing[i]) secondRingSprites[i].Texture = active;
			if (thirdRing[i]) thirdRingSprites[i].Texture = active;
			if (fourthRing[i]) fourthRingSprites[i].Texture = active;

			if (currentBeat == i)
			{
				firstRingSprites[i].Texture = current;
				secondRingSprites[i].Texture = current;
				thirdRingSprites[i].Texture = current;
				fourthRingSprites[i].Texture = current;
			}
		}
	}

	public void OnBeat()
	{
		if (firstRing[currentBeat]) firstAudioPlayer.Play();
		if (secondRing[currentBeat]) secondAudioPlayer.Play();
		if (thirdRing[currentBeat]) thirdAudioPlayer.Play();
		if (fourthRing[currentBeat]) fourthAudioPlayer.Play();
	}
}