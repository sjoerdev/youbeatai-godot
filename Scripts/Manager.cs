using Godot;
using System;

public partial class Manager : Node
{
    // textures
    [Export] Texture2D active;
    [Export] Texture2D inactive;
    [Export] Texture2D current;

	// audio
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
    Sprite2D[,] sprites;
    bool[,] rings;

    public override void _Ready()
    {
        // init rings
        rings = new bool[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                rings[ring, beat] = new Random().NextSingle() < 0.4f;
            }
        }

        // init sprites
        sprites = new Sprite2D[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                var sprite = CreateSprite(beat, ring);
                AddChild(sprite);
                sprites[ring, beat] = sprite;
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
        for (int beat = 0; beat < beatsAmount; beat++)
        {
            for (int ring = 0; ring < 4; ring++)
            {
                sprites[ring, beat].Texture = inactive;
                if (rings[ring, beat]) sprites[ring, beat].Texture = active;
                if (currentBeat == beat) sprites[ring, beat].Texture = current;
            }
        }
    }

    public void OnBeat()
    {
        if (rings[0, currentBeat]) firstAudioPlayer.Play();
        if (rings[1, currentBeat]) secondAudioPlayer.Play();
        if (rings[2, currentBeat]) thirdAudioPlayer.Play();
        if (rings[3, currentBeat]) fourthAudioPlayer.Play();
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