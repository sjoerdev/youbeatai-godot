using Godot;
using System;

public partial class Manager : Node
{
    // singleton
    public static Manager instance = null;

    // prefabs
    [Export] PackedScene spritePrefab;

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

    // beats
    Sprite2D[,] beatSprites;
    public bool[,] beatActives;

    // other
    [Export] ProgressBar progressBar;
    float progressBarValue = 0;
    [Export] Texture2D texture;
    [Export] Sprite2D pointer;
    [Export] float clapTreshold = 0.1f;
    bool clapped = false;

    public override void _Ready()
    {
        // init singleton
        instance ??= this;

        // set default actives
        beatActives = new bool[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                bool active = new Random().NextSingle() < 0.2f;
                beatActives[ring, beat] = active;
            }
        }

        // spawn sprites
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
        var timePerBeat = (60f / bpm) / 4;
        if (beatTimer > timePerBeat)
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
                var sprite = beatSprites[ring, beat];
                var active = beatActives[ring, beat];
                
                var white = new Color(1, 1, 1);
                var orange = new Color(0.8f, 0.2f, 0f);
                var blue = new Color(0, 0, 1f);

                sprite.Modulate = white;
                if (active) sprite.Modulate = orange;
                if (beat == currentBeat) sprite.Modulate = blue;

                if (sprite.Scale.X > 1) sprite.Scale -= Vector2.One * (float)delta * 0.3f;
            }
        }

        // update pointer
        float intergerFactor = (float)currentBeat / (float)beatsAmount;
        float currentBeatProgressFactor = beatTimer / timePerBeat;
        float currentbeatProgress = currentBeatProgressFactor / beatsAmount;
        float offset = timePerBeat * 2 / beatsAmount;
        float factor = intergerFactor + currentbeatProgress - offset;
        pointer.RotationDegrees = factor * 360f;

        // update progressbar
        progressBar.Value = progressBarValue;
        progressBarValue -= 0.25f * (float)delta;

        // check clap
        if (MicrophoneCapture.instance.volume > clapTreshold && clapped == false)
        {
            OnClap();
            clapped = true;
        }
    }

    public void OnClap()
    {
        GD.Print("clap");
        int ring = 1;
        bool active = beatActives[ring, currentBeat];
        var sprite = beatSprites[ring, currentBeat];
        if (active)
        {
            sprite.Scale += Vector2.One;
            progressBarValue += 1f / beatsAmount * 100f;
        }
    }

    public void OnBeat()
    {
        if (beatActives[0, currentBeat]) firstAudioPlayer.Play();
        if (beatActives[1, currentBeat]) secondAudioPlayer.Play();
        if (beatActives[2, currentBeat]) thirdAudioPlayer.Play();
        if (beatActives[3, currentBeat]) fourthAudioPlayer.Play();
        clapped = false;
    }

    private Sprite2D CreateSprite(int beat, int ring)
    {
        var sprite = (Sprite2D)spritePrefab.Instantiate();
        float angle = Mathf.Pi * 2 * beat / beatsAmount - Mathf.Pi / 2;
        float distance = (4 - ring) * 34 + 136;
        sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        sprite.Texture = texture;

        BeatSprite beatSprite = sprite as BeatSprite;
        beatSprite.spriteIndex = beat;
        beatSprite.ringIndex = ring;

        return sprite;
    }
}