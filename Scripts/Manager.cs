using Godot;
using System;

public partial class Manager : Node
{
    // singleton
    public static Manager instance = null;

    // prefabs
    [Export] PackedScene spritePrefab;

    // textures
    [Export] public Texture2D activeTexture;
    [Export] public Texture2D inactiveTexture;
    [Export] public Texture2D currentTexture;

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
    Sprite2D[,] beatSprites;
    public bool[,] beatActives;

    // other
    [Export] float clapTreshold = 0.1f;
    bool clappedThisBeat = false;

    public override void _Ready()
    {
        // init singleton
        instance ??= this;

        // init actives
        beatActives = new bool[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                beatActives[ring, beat] = new Random().NextSingle() < 0.4f;
            }
        }

        // init sprites
        beatSprites = new Sprite2D[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                var sprite = CreateSprite(beat, ring);
                AddChild(sprite);
                beatSprites[ring, beat] = sprite;
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
                beatSprites[ring, beat].Texture = inactiveTexture;
                if (beatActives[ring, beat]) beatSprites[ring, beat].Texture = activeTexture;
                if (currentBeat == beat) beatSprites[ring, beat].Texture = currentTexture;
            }
        }

        // check clap
        if (MicrophoneCapture.instance.volume > clapTreshold && clappedThisBeat == false)
        {
            OnClap();
            clappedThisBeat = true;
        }
    }

    public void OnClap()
    {
        GD.Print("clap");
        beatSprites[0, currentBeat].Scale += Vector2.One;
    }

    public void OnBeat()
    {
        if (beatActives[0, currentBeat]) firstAudioPlayer.Play();
        if (beatActives[1, currentBeat]) secondAudioPlayer.Play();
        if (beatActives[2, currentBeat]) thirdAudioPlayer.Play();
        if (beatActives[3, currentBeat]) fourthAudioPlayer.Play();
        clappedThisBeat = false;
    }

    private Sprite2D CreateSprite(int beat, int ring)
    {
        var sprite = (Sprite2D)spritePrefab.Instantiate();
        float angle = Mathf.Pi * 2 * beat / beatsAmount - Mathf.Pi / 2;
        float distance = (4 - ring) * 100 + 100;
        sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        sprite.Texture = inactiveTexture;

        BeatSprite beatSprite = sprite as BeatSprite;
        beatSprite.spriteIndex = beat;
        beatSprite.ringIndex = ring;

        return sprite;
    }
}