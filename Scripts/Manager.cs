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
    [Export] public AudioStreamPlayer2D firstAudioPlayer;
    [Export] public AudioStreamPlayer2D secondAudioPlayer;
    [Export] public AudioStreamPlayer2D thirdAudioPlayer;
    [Export] public AudioStreamPlayer2D fourthAudioPlayer;

    // saving
    [Export] AudioStream[] audioFiles;
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
    Sprite2D[,] beatSprites;
    Sprite2D[,] templateSprites;
    public bool[,] beatActives = new bool[4, 32];

    // buttons
    [Export] Button SaveLayoutButton;
    [Export] Button ClearLayoutButton;
    [Export] Button RecordButton;
    [Export] Button PlayPauseButton;
    [Export] Button BpmUpButton;
    [Export] Button BpmDownButton;

    // other
    [Export] ProgressBar progressBar;
    float progressBarValue = 0;
    [Export] Texture2D texture;
    [Export] Sprite2D pointer;
    [Export] float clapTreshold = 0.1f;
    bool clapped = false;
    public bool showTemplate = false;
    [Export] Label bpmLabel;
    [Export] Sprite2D draganddropthing;
    public bool dragginganddropping = false;
    public int holdingforring;
    [Export] public Sprite2D metronome;

    public void OnSaveLayoutButton() => TemplateManager.instance.CreateNewTemplate("custom", beatActives);
    public void OnClearLayoutButton() => beatActives = new bool[4, 32];
    public void OnRecordButton() => GD.Print("Record");
    public void OnPlayPauseButton() => playing = !playing;
    public void OnBpmUpButton() => bpm += 10;
    public void OnBpmDownButton() => bpm -= 10;

    public override void _Ready()
    {
        // init singleton
        instance ??= this;

        // init buttons
        SaveLayoutButton.Pressed += OnSaveLayoutButton;
        ClearLayoutButton.Pressed += OnClearLayoutButton;
        RecordButton.Pressed += OnRecordButton;
        PlayPauseButton.Pressed += OnPlayPauseButton;
        BpmUpButton.Pressed += OnBpmUpButton;
        BpmDownButton.Pressed += OnBpmDownButton;
        
        saveToWavButton.Pressed += SaveDrumLoopAsFile;

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

    public void SaveDrumLoopAsFile()
    {
        // set path to save to
        string pathToSaveTo = "res://savedloop.wav";
        // Check if file exists and delete it
        if (Godot.FileAccess.FileExists(pathToSaveTo)) File.Delete(pathToSaveTo);

        // set beats per drum loop
        int beatsPerDrumLoop = 32;
        // Calculate the duration of one beat in seconds
        float secondsPerBeat = (60f / bpm) / 4;
        // Calculate the total duration of the loop in seconds
        float totalDuration = beatsPerDrumLoop * secondsPerBeat;

        // Sample rate for .wav file (CD quality)
        const int sampleRate = 44100;
        // Number of samples in total
        int totalSamples = (int)(totalDuration * sampleRate);
        // Array to hold audio samples
        float[] audioData = new float[totalSamples];

        // Loop through each beat layer and beat in layer
        for (int beatLayer = 0; beatLayer < beatActives.GetLength(0); beatLayer++)
        {
            for (int beatInLayer = 0; beatInLayer < beatActives.GetLength(1); beatInLayer++)
            {
                // Check if the beat is active
                if (beatActives[beatLayer, beatInLayer])
                {
                    // Calculate the position of the sample in the audioData array
                    int startSample = (int)(beatInLayer * secondsPerBeat * sampleRate);
                    // Load the audio stream from the audio file
                    AudioStream audioStream = audioFiles[beatLayer];

                    if (audioStream is AudioStreamWav wavStream)
                    {
                        // Preload the data into memory
                        var audioByteData = wavStream.GetData();
                        // Convert audio data from byte array to float array
                        float[] sampleData = new float[audioByteData.Length / 2]; // Assuming 16-bit samples
                        for (int i = 0; i < sampleData.Length; i++)
                        {
                            // Convert bytes to short
                            short sample = (short)((audioByteData[i * 2 + 1] << 8) | (audioByteData[i * 2] & 0xFF));
                            // Normalize to -1.0 to 1.0
                            sampleData[i] = sample / (float)short.MaxValue;
                        }

                        // Mix the audio sample into the audioData array
                        for (int sampleIndex = 0; sampleIndex < sampleData.Length; sampleIndex++)
                        {
                            // Calculate the sample position
                            int samplePos = startSample + sampleIndex;
                            // Check if within bounds of audioData
                            if (samplePos < totalSamples)
                            {
                                // Mix the samples, ensuring we do not exceed the maximum float value
                                audioData[samplePos] += sampleData[sampleIndex];
                            }
                        }
                    }
                    else
                    {
                        GD.PrintErr("Unsupported AudioStream type.");
                    }
                }
            }
        }

        // Normalize the audio data to avoid clipping
        float maxAmplitude = 0;

        // Find the maximum amplitude in the audio data
        foreach (var sample in audioData)
        {
            if (Math.Abs(sample) > maxAmplitude)
            {
                maxAmplitude = Math.Abs(sample);
            }
        }

        // Normalize if necessary
        if (maxAmplitude > 1.0f)
        {
            for (int i = 0; i < audioData.Length; i++)
            {
                audioData[i] /= maxAmplitude;
            }
        }

        // Save the audio data as a .wav file using Godot's FileAccess class
        Godot.FileAccess file = Godot.FileAccess.Open(pathToSaveTo, Godot.FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr("Failed to open file for writing.");
            return;
        }

        // Prepare the WAV header
        int byteRate = sampleRate * 2; // 16 bits = 2 bytes
        int dataSize = audioData.Length * 2; // Size of the audio data in bytes

        // Write the WAV header
        file.StoreString("RIFF");
        file.Store32((uint)(36 + dataSize)); // Chunk size
        file.StoreString("WAVE");
        file.StoreString("fmt ");
        file.Store32(16); // Subchunk1 size
        file.Store16(1); // Audio format (PCM)
        file.Store16(1); // Number of channels
        file.Store32((uint)sampleRate); // Sample rate
        file.Store32((uint)byteRate); // Byte rate
        file.Store16(2); // Block align
        file.Store16(16); // Bits per sample
        file.StoreString("data");
        file.Store32((uint)dataSize); // Subchunk2 size

        // Write audio data
        foreach (var sample in audioData)
        {
            short intSample = (short)(sample * short.MaxValue);
            byte[] byteSample = BitConverter.GetBytes(intSample);
            file.StoreBuffer(byteSample);
        }

        file.Close(); // Close the file
        GD.Print("Drum loop saved successfully!");
    }

    public override void _Process(double delta)
    {
        // drag&drop
        if (dragginganddropping)
        {
            draganddropthing.Modulate = colors[holdingforring];
            draganddropthing.Position = GetViewport().GetMousePosition() - (DisplayServer.WindowGetSize() / 2);
        }
        else draganddropthing.Modulate = new Color(1, 1, 1, 0);

        if (playing)
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

            // Metronome
            var beatprogress = beatTimer / timePerBeat;
            metronome.Position = new Vector2(metronome.Position.X, Mathf.Lerp(-162, -100, (Mathf.Sin(beatprogress * Mathf.Pi * 2) + 1) / 2));

            // update pointer
            float intergerFactor = (float)currentBeat / (float)beatsAmount;
            float perbeat = (60f / bpm) / 4;
            float currentBeatProgressFactor = beatTimer / perbeat;
            float currentbeatProgress = currentBeatProgressFactor / beatsAmount;
            float factor = intergerFactor + currentbeatProgress;
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

        // update sprites
        for (int beat = 0; beat < beatsAmount; beat++)
        {
            for (int ring = 0; ring < 4; ring++)
            {
                var sprite = beatSprites[ring, beat];
                var active = beatActives[ring, beat];

                var color = colors[ring] / 2;

                if (active) color *= 2;
                if (beat == currentBeat) color *= 2;

                sprite.Modulate = color;

                if (sprite.Scale.X > 1) sprite.Scale -= Vector2.One * (float)delta * 0.3f;
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