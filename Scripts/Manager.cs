using Godot;
using System;
using System.Collections.Generic;
using System.IO;

using NAudio.Wave;
using NAudio.Lame;

public partial class Manager : Node
{
    // singleton
    public static Manager instance = null;

	// audio
    public AudioStreamPlayer2D firstAudioPlayer;
    public AudioStreamPlayer2D secondAudioPlayer;
    public AudioStreamPlayer2D thirdAudioPlayer;
    public AudioStreamPlayer2D fourthAudioPlayer;

    // other sfx
    AudioStreamPlayer2D extraAudioPlayer;
    [Export] AudioStream metronome_sfx;
    [Export] AudioStream achievement_sfx;
    bool metronome_sfx_enabled = false;

    // saving
    [Export] public AudioStream[] mainAudioFiles;
    public AudioStream[] audioFilesToUse;
    [Export] Button saveToWavButton;
    bool hassavedtowav = false;

    // timing
    bool playing = false;
    [Export] public int bpm = 120;
    [Export] int beatsAmount = 32;
    public int currentBeat = 0;
    float beatTimer = 0;
    float slowBeatTimer = 0;

    // particles
    [Export] public CpuParticles2D beat_particles;
    private Vector2 beat_particles_position;
    private float beat_particles_time;
    private float beat_particles_curtime;
    private Color beat_particles_color;
    private bool beat_particles_emitting = false;

    [Export] public CpuParticles2D pbar_particles;
    private float pbar_particles_time;
    private float pbar_particles_curtime;
    private bool pbar_particles_emitting = false;

    [Export] public CpuParticles2D achievement_particles;
    private float achievement_particles_time;
    private float achievement_particles_curtime;
    private bool achievement_particles_emitting = false;

    public void EmitBeatParticles(Vector2 position, Color color)
    {
        beat_particles_curtime = 0;
        beat_particles_time = 0.05f;
        beat_particles_position = position;
        beat_particles_color = color.Lightened(0.25f);
        beat_particles_emitting = true;
    }

    public void EmitProgressBarParticles()
    {
        pbar_particles_curtime = 0;
        pbar_particles_time = 0.1f;
        pbar_particles_emitting = true;
    }

    public void EmitAchievementParticles()
    {
        achievement_particles_curtime = 0;
        achievement_particles_time = 0.5f;
        achievement_particles_emitting = true;
    }

    // colors
    [Export] public Color[] colors;

    // beats
    [Export] PackedScene spritePrefab;
    [Export] Texture2D texture;
    [Export] Texture2D outline;
    Sprite2D[,] beatOutlines;
    public Sprite2D[,] beatSprites;
    Sprite2D[,] templateSprites;
    public bool[,] beatActives = new bool[4, 32];

    // left buttons
    [Export] Button SaveLayoutButton;
    [Export] Button ClearLayoutButton;
    [Export] Button RecordButton;
    [Export] Button PlayPauseButton;
    [Export] Button ResetPlayerButton;
    [Export] Button BpmUpButton;
    [Export] Button BpmDownButton;

    // sample buttons
    [Export] public Sprite2D draganddropButton0;
    [Export] public Sprite2D draganddropButton1;
    [Export] public Sprite2D draganddropButton2;
    [Export] public Sprite2D draganddropButton3;
    [Export] public RecordSampleButton recordSampleButton0;
    [Export] public RecordSampleButton recordSampleButton1;
    [Export] public RecordSampleButton recordSampleButton2;
    [Export] public RecordSampleButton recordSampleButton3;
    [Export] public CheckButton recordSampleCheckButton0;
    [Export] public CheckButton recordSampleCheckButton1;
    [Export] public CheckButton recordSampleCheckButton2;
    [Export] public CheckButton recordSampleCheckButton3;

    // settings menu buttons
    [Export] Button metronome_sfx_toggle_button;

    // other interface
    [Export] Button skiptutorialbutton;
    [Export] ProgressBar progressBar;
    float progressBarValue = 0;
    [Export] Sprite2D pointer;
    [Export] public Sprite2D metronome;
    [Export] public Sprite2D metronomebg;
    [Export] Label bpmLabel;
    [Export] Sprite2D draganddropthing;
    public bool dragginganddropping = false;
    public int holdingforring;
    [Export] float swing = 0.5f;
    [Export] Slider swingslider;
    [Export] Label swinglabel;
    [Export] Button settingsButton;
    bool showsettingsmenu = false;
    [Export] Panel settingsPanel;

    // clapping and stomping
    bool stomped = false;
    bool clapped = false;
    int clappedAmount = 0;
    int stompedAmount = 0;

    // other
    public bool showTemplate = false;
    public bool selectedTemplate = false;
    bool haschangedbpm = false;
    bool hasclearedlayout = false;
    private bool spacedownlastframe = false;
    private bool enterdownlastframe = false;
    float timeafterplay = 0;
    [Export] Slider ClapBiasSlider;
    [Export] Panel instructionspanel;

    // on button functions
    public void OnSaveLayoutButton() => TemplateManager.instance.CreateNewTemplate("custom", beatActives);
    public void OnClearLayoutButton()
    {
        beatActives = new bool[4, 32];
        hasclearedlayout = true;
    }
    public void OnRecordButton() => GD.Print("Record");
    public void OnPlayPauseButton() => playing = !playing;
    public void OnBpmUpButton()
    {
        bpm += 10;
        haschangedbpm = true;
    }
    public void OnBpmDownButton()
    {
        bpm -= 10;
        haschangedbpm = true;
    }
    public void OnResetPlayerButton() => currentBeat = 0;

    private void PlayExtraSFX(AudioStream audioStream)
    {
        extraAudioPlayer.Stop();
        extraAudioPlayer.Stream = audioStream;
        extraAudioPlayer.Play();
    }

    // on toggle functions
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
        extraAudioPlayer = new AudioStreamPlayer2D();
        firstAudioPlayer = new AudioStreamPlayer2D();
        secondAudioPlayer = new AudioStreamPlayer2D();
        thirdAudioPlayer = new AudioStreamPlayer2D();
        fourthAudioPlayer = new AudioStreamPlayer2D();
        AddChild(extraAudioPlayer);
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
        settingsButton.Pressed += () => showsettingsmenu = !showsettingsmenu;
        metronome_sfx_toggle_button.Pressed += () => metronome_sfx_enabled = !metronome_sfx_enabled;
        SaveLayoutButton.Pressed += OnSaveLayoutButton;
        ClearLayoutButton.Pressed += OnClearLayoutButton;
        RecordButton.Pressed += OnRecordButton;
        PlayPauseButton.Pressed += OnPlayPauseButton;
        BpmUpButton.Pressed += OnBpmUpButton;
        BpmDownButton.Pressed += OnBpmDownButton;
        saveToWavButton.Pressed += SaveDrumLoopAsFile;
        ResetPlayerButton.Pressed += () => { OnResetPlayerButton(); playing = true; };
        skiptutorialbutton.Pressed += () => GD.Print("todo");

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

    [Export] Label InstructionLabel;
    int level = 0;

    // arrow keys
    bool up_pressed = false;
	bool up_pressed_lastframe = false;
    bool dn_pressed = false;
	bool dn_pressed_lastframe = false;
    bool lf_pressed = false;
	bool lf_pressed_lastframe = false;
    bool rt_pressed = false;
	bool rt_pressed_lastframe = false;

    public override void _Process(double delta)
    {
        // deal with beat particles
        if (beat_particles_emitting && beat_particles_curtime < beat_particles_time)
        {
            beat_particles.Color = beat_particles_color;
            beat_particles.Position = beat_particles_position;
            beat_particles.Emitting = true;
            beat_particles_curtime += (float)delta;
        }
        else
        {
            beat_particles.Emitting = false;
            beat_particles_emitting = false;
        }

        // deal with progress bar particles
        if (pbar_particles_emitting && pbar_particles_curtime < pbar_particles_time)
        {
            pbar_particles.Emitting = true;
            pbar_particles_curtime += (float)delta;
        }
        else
        {
            pbar_particles.Emitting = false;
            pbar_particles_emitting = false;
        }

        // deal with progress bar particles
        if (achievement_particles_emitting && achievement_particles_curtime < achievement_particles_time)
        {
            achievement_particles.Emitting = true;
            achievement_particles_curtime += (float)delta;
        }
        else
        {
            achievement_particles.Emitting = false;
            achievement_particles_emitting = false;
        }

        // deal with arrowkeys
        up_pressed_lastframe = up_pressed;
		up_pressed = Input.IsKeyPressed(Key.Up);
		if (up_pressed && up_pressed != up_pressed_lastframe) OnBpmUpButton();
        dn_pressed_lastframe = dn_pressed;
		dn_pressed = Input.IsKeyPressed(Key.Down);
		if (dn_pressed && dn_pressed != dn_pressed_lastframe) OnBpmDownButton();
        
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
            timeafterplay += ((float)delta);

            // keep time (with swing)
            beatTimer += (float)delta;
            var baseTimePerBeat = (60f / bpm) / 2;
            var timePerBeat = (currentBeat % 2 == 1) ? baseTimePerBeat * (1 + swing) : baseTimePerBeat * (1 - (swing / 2));
            if (beatTimer > timePerBeat)
            {
                beatTimer -= timePerBeat;
                currentBeat = (currentBeat + 1) % beatsAmount;
                OnBeat();
            }

            slowBeatTimer += (float)delta / 4;
            if (slowBeatTimer > timePerBeat) slowBeatTimer -= timePerBeat;

            // metronome
            var beatprogress = slowBeatTimer / timePerBeat;
            metronome.Position = new Vector2(metronome.Position.X, Mathf.Lerp(-0.4f, 0.4f, beatprogress));

            // update progressbar
            progressBar.Value = progressBarValue;
            if (progressBarValue > 0) progressBarValue -= 0.25f * (float)delta;

            // blip
            var volume = MicrophoneCapture.instance.volume;
            var frequency = MicrophoneCapture.instance.frequency;

            //if (volume > 0.1f) GD.Print(frequency);

            // check clap
            if (volume > 0.1f && frequency > ClapBiasSlider.Value && clapped == false)
            {
                OnClap();
                clapped = true;
            }

            // check stomp
            if (volume > 0.1f && frequency < ClapBiasSlider.Value && stomped == false)
            {
                OnStomp();
                stomped = true;
            }
        }
        else timeafterplay = 0;

        // update sprites
        for (int beat = 0; beat < beatsAmount; beat++)
        {
            for (int ring = 0; ring < 4; ring++)
            {
                var sprite = beatSprites[ring, beat];
                var active = beatActives[ring, beat];

                var color = colors[ring];

                if (beat == currentBeat)
                {
                    if (active) color = color.Lightened(0.75f);
                    else color = new(1, 1, 1, 0.5f);
                }
                else if (!active) color.A = 0.2f;

                sprite.Modulate = color;

                if (sprite.Scale.X > 1) sprite.Scale -= Vector2.One * (float)delta * 0.3f;
            }
        }

        // bloop
        if (draganddropButton0.Scale.X > 2) draganddropButton0.Scale -= Vector2.One * (float)delta * 0.8f;
        if (draganddropButton1.Scale.X > 2) draganddropButton1.Scale -= Vector2.One * (float)delta * 0.8f;
        if (draganddropButton2.Scale.X > 2) draganddropButton2.Scale -= Vector2.One * (float)delta * 0.8f;
        if (draganddropButton3.Scale.X > 2) draganddropButton3.Scale -= Vector2.One * (float)delta * 0.8f;

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
        string sanitizedTime = Time.GetTimeStringFromSystem().Replace(":", "-");
        string filename = "savedloop_" + bpm.ToString() + "bpm_" + sanitizedTime;

        int sampleRate = 44100;
        float secondsPerBeat = (60f / bpm) / 2;
        int totalSamples = (int)(beatsAmount * secondsPerBeat * sampleRate);
        float[] audioData = new float[totalSamples];

        // audiodata
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

        // normalize
        float maxAmplitude = 0;
        foreach (var sample in audioData) if (Math.Abs(sample) > maxAmplitude) maxAmplitude = Math.Abs(sample);
        if (maxAmplitude > 1.0f) for (int i = 0; i < audioData.Length; i++) audioData[i] /= maxAmplitude;

        // write wave
        using (var writer = new WaveFileWriter(filename + ".wav", new WaveFormat(sampleRate, 1)))
        {
            foreach (var sample in audioData)
            {
                short intSample = (short)(sample * short.MaxValue);
                writer.WriteSample(intSample / (float)short.MaxValue);
            }
            writer.Close();
        }

        GD.Print("Drum loop saved as WAV successfully!");
        ConvertWavToMp3(filename);
        GD.Print("Drum loop converted to MP3 successfully!");
    }

    private void ConvertWavToMp3(string filename)
    {
        var reader = new AudioFileReader(filename + ".wav");
        var writer = new LameMP3FileWriter(filename + ".mp3", reader.WaveFormat, LAMEPreset.STANDARD);
        reader.CopyTo(writer);
        reader.Close();
        writer.Close();
        File.Delete(filename + ".wav");
    }

    public void OnClap()
    {
        if (timeafterplay < 0.2f) return;
        GD.Print("clap");
        int ring = 1;
        bool active = beatActives[ring, currentBeat];
        var sprite = beatSprites[ring, currentBeat];
        if (active)
        {
            sprite.Scale += Vector2.One;
            progressBarValue += 1f / beatsAmount * 100f;
            EmitProgressBarParticles();
        }
        clappedAmount++;
        draganddropButton1.Scale += Vector2.One / 2;
        ((DragAndDropButton)draganddropButton1).ActivateBeat();
    }

    public void OnStomp()
    {
        if (timeafterplay < 0.2f) return;
        GD.Print("stomp");
        int ring = 0;
        bool active = beatActives[ring, currentBeat];
        var sprite = beatSprites[ring, currentBeat];
        if (active)
        {
            sprite.Scale += Vector2.One;
            progressBarValue += 1f / beatsAmount * 100f;
            EmitProgressBarParticles();
        }
        stompedAmount++;
        draganddropButton0.Scale += Vector2.One / 2;
        ((DragAndDropButton)draganddropButton0).ActivateBeat();
    }

    public void OnBeat()
    {
        if (metronome_sfx_enabled && currentBeat % 2 == 0) PlayExtraSFX(metronome_sfx);
        if (beatActives[0, currentBeat]) firstAudioPlayer.Play();
        if (beatActives[1, currentBeat]) secondAudioPlayer.Play();
        if (beatActives[2, currentBeat]) thirdAudioPlayer.Play();
        if (beatActives[3, currentBeat]) fourthAudioPlayer.Play();
        clapped = false;
        stomped = false;
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
        beatSprite.ring = ring;

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

    int AmountOfActives(int ring)
    {
        int amount = 0;
        for (int beat = 0; beat < beatsAmount; beat++) if (beatActives[ring, beat]) amount++;
        return amount;
    }
}