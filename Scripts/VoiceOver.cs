using Godot;
using System;

public partial class VoiceOver : Node
{
	// singleton
    public static VoiceOver instance = null;

	[Export] Button recordButton;
	bool shouldRecord = false;

	[Export] Button snellerButton;
	[Export] Button langzamerButton;

	[Export] TextureProgressBar textureProgressBar;

    AudioEffectRecord audioEffectRecord;
	AudioStreamPlayer2D audioPlayer;
	bool recording = false;
	float recordingTimer = 0;

	AudioStream[] voiceOvers = new AudioStream[10];

	int currentLayer => Manager.instance.currentLayerIndex;

	public void SetCurrentLayerVoiceOver(AudioStream voiceOver) => voiceOvers[currentLayer] = voiceOver;
	public AudioStream GetCurrentLayerVoiceOver() => voiceOvers[currentLayer];

	public override void _Ready()
    {
		// init singleton
        instance ??= this;

		// init record button
		recordButton.Pressed += () => shouldRecord = !shouldRecord;

		// create audioplayer
		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		// setup record effect
        audioEffectRecord = (AudioEffectRecord)AudioServer.GetBusEffect(AudioServer.GetBusIndex("Microphone"), 1);
    }

    public override void _Process(double delta)
	{
		// set color of record button
		recordButton.Modulate = shouldRecord ? new(1, 0.5f, 0.5f) : new(1, 1, 1);

		// disable bpm buttons during recording
		snellerButton.Disabled = recording;
		langzamerButton.Disabled = recording;

		// update recording timer
		if (recording) recordingTimer += (float)delta;
		else recordingTimer = 0;

		// debug record keys
		if (Input.IsKeyPressed(Key.Left) && !recording) StartRecording();
		if (Input.IsKeyPressed(Key.Right) && recording) StopRecording();

		// set progress bar value
		float secondsPerBeat = (60f / Manager.instance.bpm) / 2;
		float secondsPerRotation = secondsPerBeat * 32;
		float bpmfactor = 32 / secondsPerRotation;
		if (recording) textureProgressBar.Value = recordingTimer * bpmfactor;
		else
		{
			if (GetCurrentLayerVoiceOver() != null) textureProgressBar.Value = 32f;
			else textureProgressBar.Value = 0;
		}
	}

	public void OnTop()
	{
		if (audioPlayer.Playing) audioPlayer.Stop();

		if (shouldRecord && !recording) StartRecording();
		else if (recording) StopRecording();

		if (!recording)
		{
			audioPlayer.Stream = GetCurrentLayerVoiceOver();
			audioPlayer.Play();
		}
	}

    private void StartRecording()
    {
		recording = true;
        audioEffectRecord.SetRecordingActive(true);
		GD.Print("recording started");
    }

    private void StopRecording()
    {
        audioEffectRecord.SetRecordingActive(false);
		GD.Print("recording stopped");
		recording = false;
		shouldRecord = false;
		SetCurrentLayerVoiceOver(audioEffectRecord.GetRecording());
    }
}