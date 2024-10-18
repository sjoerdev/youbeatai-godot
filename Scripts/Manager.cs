using Godot;
using System;

public partial class Manager : Node
{
    // textures
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
    float beatTimer = 0;

	// rings
    Sprite2D[][] ringSprites;
    bool[][] rings;

    public override void _Ready()
    {
		// init rings
        var random = new Random();
        float chance = 0.4f;
		rings = new bool[4][];
        for (int ringIndex = 0; ringIndex < 4; ringIndex++)
        {
            rings[ringIndex] = new bool[beatsAmount];
            for (int i = 0; i < beatsAmount; i++)
            {
                rings[ringIndex][i] = random.NextSingle() < chance;
            }
        }

		// init sprites
		ringSprites = new Sprite2D[4][];
        for (int ringIndex = 0; ringIndex < 4; ringIndex++)
        {
            ringSprites[ringIndex] = new Sprite2D[beatsAmount];
            for (int i = 0; i < beatsAmount; i++)
            {
                var sprite = CreateSprite(i, ringIndex);
                AddChild(sprite);
                ringSprites[ringIndex][i] = sprite;
            }
        }
    }

    public override void _Process(double delta)
    {
        // keep time
        beatTimer += (float)delta;
        if (beatTimer > (60f / bpm) / 4)
        {
            beatTimer = 0;
            currentBeat = (currentBeat + 1) % beatsAmount;
            OnBeat();
        }

        // update sprites
		for (int i = 0; i < beatsAmount; i++)
        {
            for (int ringIndex = 0; ringIndex < 4; ringIndex++)
            {
                ringSprites[ringIndex][i].Texture = inactive;
                if (rings[ringIndex][i]) ringSprites[ringIndex][i].Texture = active;
                if (currentBeat == i) ringSprites[ringIndex][i].Texture = current;
            }
        }
    }

    public void OnBeat()
    {
		if (rings[0][currentBeat]) firstAudioPlayer.Play();
		if (rings[1][currentBeat]) secondAudioPlayer.Play();
		if (rings[2][currentBeat]) thirdAudioPlayer.Play();
		if (rings[3][currentBeat]) fourthAudioPlayer.Play();
    }

	private Sprite2D CreateSprite(int i, int ringIndex)
    {
        var sprite = new Sprite2D();
        float angle = Mathf.Pi * 2 * i / beatsAmount - Mathf.Pi / 2;
        float distance = (4 - ringIndex) * 100 + 100;
        sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        sprite.Texture = inactive;
        return sprite;
    }
}