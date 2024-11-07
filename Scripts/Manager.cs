using Godot;
using System;
using System.IO;

public partial class Manager : Node
{
    // singleton
    public static Manager instance = null;

    // prefabs
    [Export] PackedScene spritePrefab;

	// audio
    public AudioStreamPlayer2D firstAudioPlayer;
    public AudioStreamPlayer2D secondAudioPlayer;
    public AudioStreamPlayer2D thirdAudioPlayer;
    public AudioStreamPlayer2D fourthAudioPlayer;

    // saving
    [Export] public AudioStream[] mainAudioFiles;
    public AudioStream[] audioFilesToUse;
    [Export] Button saveToWavButton;

    // timing
    bool playing = false;
    [Export] public int bpm = 120;
    [Export] int beatsAmount = 32;
    public int currentBeat = 0;
    float beatTimer = 0;

    // colors
    [Export] public Color[] colors;

    // beats
    Sprite2D[,] beatOutlines;
    Sprite2D[,] beatSprites;
    Sprite2D[,] templateSprites;
    public bool[,] beatActives = new bool[4, 32];

    // buttons
    [Export] Button SaveLayoutButton;
    [Export] Button ClearLayoutButton;
    [Export] Button RecordButton;
    [Export] Button PlayPauseButton;
    [Export] Button ResetPlayerButton;
    [Export] Button BpmUpButton;
    [Export] Button BpmDownButton;

    // recordsamplebuttons
    [Export] public RecordSampleButton recordSampleButton0;
    [Export] public RecordSampleButton recordSampleButton1;
    [Export] public RecordSampleButton recordSampleButton2;
    [Export] public RecordSampleButton recordSampleButton3;
    [Export] public CheckButton recordSampleCheckButton0;
    [Export] public CheckButton recordSampleCheckButton1;
    [Export] public CheckButton recordSampleCheckButton2;
    [Export] public CheckButton recordSampleCheckButton3;

    // other
    [Export] ProgressBar progressBar;
    float progressBarValue = 0;
    [Export] Texture2D texture;
    [Export] Texture2D outline;
    [Export] Sprite2D pointer;
    [Export] float clapTreshold = 0.1f;
    bool clapped = false;
    public bool showTemplate = false;
    [Export] Label bpmLabel;
    [Export] Sprite2D draganddropthing;
    public bool dragginganddropping = false;
    public int holdingforring;
    [Export] public Sprite2D metronome;

    [Export] float swing = 0.5f;
    [Export] Slider swingslider;

    public void OnSaveLayoutButton() => TemplateManager.instance.CreateNewTemplate("custom", beatActives);
    public void OnClearLayoutButton() => beatActives = new bool[4, 32];
    public void OnRecordButton() => GD.Print("Record");
    public void OnPlayPauseButton() => playing = !playing;
    public void OnBpmUpButton() => bpm += 10;
    public void OnBpmDownButton() => bpm -= 10;
    public void OnResetPlayerButton() => currentBeat = 0;

    private void OnToggled0(bool toggledOn)
    {
        firstAudioPlayer.Stop();
        audioFilesToUse[0] = toggledOn ? recordSampleButton0.recordedAudio : mainAudioFiles[0];
        firstAudioPlayer.Stream = audioFilesToUse[0];
    }

    private void OnToggled1(bool toggledOn)
    {
        secondAudioPlayer.Stop();
        audioFilesToUse[1] = toggledOn ? recordSampleButton1.recordedAudio : mainAudioFiles[1];
        secondAudioPlayer.Stream = audioFilesToUse[1];
    }

    private void OnToggled2(bool toggledOn)
    {
        thirdAudioPlayer.Stop();
        audioFilesToUse[2] = toggledOn ? recordSampleButton2.recordedAudio : mainAudioFiles[2];
        thirdAudioPlayer.Stream = audioFilesToUse[2];
    }

    private void OnToggled3(bool toggledOn)
    {
        fourthAudioPlayer.Stop();
        audioFilesToUse[3] = toggledOn ? recordSampleButton3.recordedAudio : mainAudioFiles[3];
        fourthAudioPlayer.Stream = audioFilesToUse[3];  
    }

    public override void _Ready()
    {
        // init singleton
        instance ??= this;

        // init audioplayers
        firstAudioPlayer = new AudioStreamPlayer2D();
        secondAudioPlayer = new AudioStreamPlayer2D();
        thirdAudioPlayer = new AudioStreamPlayer2D();
        fourthAudioPlayer = new AudioStreamPlayer2D();
        AddChild(firstAudioPlayer);
        AddChild(secondAudioPlayer);
        AddChild(thirdAudioPlayer);
        AddChild(fourthAudioPlayer);

        audioFilesToUse = (AudioStream[])mainAudioFiles.Clone();
        firstAudioPlayer.Stream = mainAudioFiles[0];
        secondAudioPlayer.Stream = mainAudioFiles[1];
        thirdAudioPlayer.Stream = mainAudioFiles[2];
        fourthAudioPlayer.Stream = mainAudioFiles[3];

        // init buttons
        SaveLayoutButton.Pressed += OnSaveLayoutButton;
        ClearLayoutButton.Pressed += OnClearLayoutButton;
        RecordButton.Pressed += OnRecordButton;
        PlayPauseButton.Pressed += OnPlayPauseButton;
        BpmUpButton.Pressed += OnBpmUpButton;
        BpmDownButton.Pressed += OnBpmDownButton;
        saveToWavButton.Pressed += SaveDrumLoopAsFile;
        ResetPlayerButton.Pressed += () => { OnResetPlayerButton(); playing = true; };

        // checkbuttons
        recordSampleCheckButton0.Toggled += OnToggled0;
        recordSampleCheckButton1.Toggled += OnToggled1;
        recordSampleCheckButton2.Toggled += OnToggled2;
        recordSampleCheckButton3.Toggled += OnToggled3;

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

        // spawn outlines
        beatOutlines = new Sprite2D[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                var outline = CreateOutline(beat, ring);
                AddChild(outline);
                beatOutlines[ring, beat] = outline;
            }
        }

        // spawn template sprites
        templateSprites = new Sprite2D[4, beatsAmount];
        for (int ring = 0; ring < 4; ring++)
        {
            for (int beat = 0; beat < beatsAmount; beat++)
            {
                var sprite = CreateTemplateSprite(beat, ring);
                AddChild(sprite);
                templateSprites[ring, beat] = sprite;
            }
        }
    }

    private bool spacedownlastframe = false;
    private bool enterdownlastframe = false;

    public override void _Process(double delta)
    {
        // update swing amount
        swing = (float)swingslider.Value;

        // space as play/pause
        var spacedown = Input.IsKeyPressed(Key.Space);
        if (spacedown && spacedownlastframe == false) OnPlayPauseButton();
        spacedownlastframe = spacedown;

        // enter as reset player
        var enterdown = Input.IsKeyPressed(Key.Enter);
        if (enterdown && enterdownlastframe == false) { OnResetPlayerButton(); playing = true; }
        enterdownlastframe = enterdown;

        // drag&drop
        if (dragginganddropping)
        {
            draganddropthing.Modulate = colors[holdingforring];
            draganddropthing.Position = GetViewport().GetMousePosition() - (DisplayServer.WindowGetSize() / 2);
        }
        else draganddropthing.Modulate = new Color(1, 1, 1, 0);

        // update pointer
        float intergerFactor = (float)currentBeat / (float)beatsAmount;
        pointer.RotationDegrees = intergerFactor * 360f;

        if (playing)
        {
            // Keep time (with swing)
            beatTimer += (float)delta;
            var baseTimePerBeat = (60f / bpm) / 4;
            var timePerBeat = (currentBeat % 2 == 1) ? baseTimePerBeat * (1 + swing) : baseTimePerBeat * (1 - (swing / 2));
            if (beatTimer > timePerBeat)
            {
                beatTimer -= timePerBeat;
                currentBeat = (currentBeat + 1) % beatsAmount;
                OnBeat();
            }

            // Metronome
            var beatprogress = beatTimer / timePerBeat;
            metronome.Position = new Vector2(metronome.Position.X, Mathf.Lerp(-0.4f, 0.4f, (Mathf.Sin(beatprogress * Mathf.Pi * 2) + 1) / 2));

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

        // update sprites
        for (int beat = 0; beat < beatsAmount; beat++)
        {
            for (int ring = 0; ring < 4; ring++)
            {
                var sprite = beatSprites[ring, beat];
                var active = beatActives[ring, beat];

                var color = colors[ring];

                if (beat == currentBeat) color = color.Lightened(2);
                else if (!active) color.A = 0.2f;

                sprite.Modulate = color;

                if (sprite.Scale.X > 1) sprite.Scale -= Vector2.One * (float)delta * 0.3f;
            }
        }

        // update outlines
        for (int beat = 0; beat < beatsAmount; beat++)
        {
            for (int ring = 0; ring < 4; ring++)
            {
                var outline = beatOutlines[ring, beat];
                outline.Modulate = colors[ring];
            }
        }

        // update template sprites
        for (int beat = 0; beat < beatsAmount; beat++)
        {
            for (int ring = 0; ring < 4; ring++)
            {
                var sprite = templateSprites[ring, beat];
                var active = TemplateManager.instance.GetCurrentActives()[ring, beat];
                sprite.Modulate = new Color(0, 0, 0, 0);
                if (active && showTemplate) sprite.Modulate = new Color(0, 0, 0, 1);
            }
        }

        // update bpm label
        bpmLabel.Text = bpm.ToString();
    }

    public void SaveDrumLoopAsFile()
    {
        string pathToSaveTo = "savedloop" + "_" + bpm.ToString() + "bpm" + ".wav";
        if (Godot.FileAccess.FileExists(pathToSaveTo)) File.Delete(pathToSaveTo);
        
        float secondsPerBeat = (60f / bpm) / 4;
        float totalDuration = beatsAmount * secondsPerBeat;

        int sampleRate = 44100;
        int totalSamples = (int)(totalDuration * sampleRate);
        float[] audioData = new float[totalSamples];

        for (int ring = 0; ring < beatActives.GetLength(0); ring++)
        {
            for (int beat = 0; beat < beatActives.GetLength(1); beat++)
            {
                if (beatActives[ring, beat])
                {
                    var audioByteData = ((AudioStreamWav)audioFilesToUse[ring]).GetData();
                    float[] sampleData = new float[audioByteData.Length / 2];
                    for (int i = 0; i < sampleData.Length; i++) sampleData[i] = (short)((audioByteData[i * 2 + 1] << 8) | (audioByteData[i * 2] & 0xFF)) / (float)short.MaxValue;
                    for (int sampleIndex = 0; sampleIndex < sampleData.Length; sampleIndex++)
                    {
                        int samplePos = (int)(beat * secondsPerBeat * sampleRate) + sampleIndex;
                        if (samplePos < totalSamples) audioData[samplePos] += sampleData[sampleIndex];
                    }
                }
            }
        }

        float maxAmplitude = 0;
        foreach (var sample in audioData) if (Math.Abs(sample) > maxAmplitude) maxAmplitude = Math.Abs(sample);
        if (maxAmplitude > 1.0f) for (int i = 0; i < audioData.Length; i++) audioData[i] /= maxAmplitude;

        Godot.FileAccess file = Godot.FileAccess.Open(pathToSaveTo, Godot.FileAccess.ModeFlags.Write);
        int byteRate = sampleRate * 2;
        int dataSize = audioData.Length * 2;
        file.StoreString("RIFF");
        file.Store32((uint)(36 + dataSize));
        file.StoreString("WAVE");
        file.StoreString("fmt ");
        file.Store32(16);
        file.Store16(1);
        file.Store16(1);
        file.Store32((uint)sampleRate);
        file.Store32((uint)byteRate);
        file.Store16(2);
        file.Store16(16);
        file.StoreString("data");
        file.Store32((uint)dataSize);
        foreach (var sample in audioData)
        {
            short intSample = (short)(sample * short.MaxValue);
            byte[] byteSample = BitConverter.GetBytes(intSample);
            file.StoreBuffer(byteSample);
        }

        file.Close();
        GD.Print("Drum loop saved successfully!");
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

    private Sprite2D CreateOutline(int beat, int ring)
    {
        var sprite = new Sprite2D();
        float angle = Mathf.Pi * 2 * beat / beatsAmount - Mathf.Pi / 2;
        float distance = (4 - ring) * 30 + 110;
        sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        sprite.Texture = outline;
        return sprite;
    }

    private Sprite2D CreateSprite(int beat, int ring)
    {
        var sprite = (Sprite2D)spritePrefab.Instantiate();
        float angle = Mathf.Pi * 2 * beat / beatsAmount - Mathf.Pi / 2;
        float distance = (4 - ring) * 30 + 110;
        sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        sprite.Texture = texture;

        BeatSprite beatSprite = sprite as BeatSprite;
        beatSprite.spriteIndex = beat;
        beatSprite.ringIndex = ring;

        return sprite;
    }

    private Sprite2D CreateTemplateSprite(int beat, int ring)
    {
        var sprite = new Sprite2D();
        float angle = Mathf.Pi * 2 * beat / beatsAmount - Mathf.Pi / 2;
        float distance = (4 - ring) * 30 + 110;
        sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        sprite.Texture = texture;
        sprite.Modulate = new Color(0, 0, 0, 1);
        sprite.Scale = Vector2.One * 0.2f;
        return sprite;
    }
}