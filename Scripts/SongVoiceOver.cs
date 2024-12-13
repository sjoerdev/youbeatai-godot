using Godot;
using System;

public partial class SongVoiceOver : Node
{
	// singleton
    public static SongVoiceOver instance = null;

	// user interface
	[Export] ProgressBar progressbar;
	[Export] public Button recordButton;

	// recording
	AudioStream voiceOver;
    AudioEffectRecord audioEffectRecord;
	AudioStreamPlayer2D audioPlayer;
	bool shouldRecord = false;
	public bool recording = false;
	float recordingTimer = 0;

	// other
	[Export] Button snellerButton;
	[Export] Button langzamerButton;

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

		// update recording timer
		if (recording) recordingTimer += (float)delta;
		else recordingTimer = 0;

		// buttons during recording
		snellerButton.Disabled = recording;
		langzamerButton.Disabled = recording;
		Manager.instance.SetLayerSwitchButtonsEnabled(!recording);
		Manager.instance.PlayPauseButton.Disabled = recording;
		Manager.instance.ResetPlayerButton.Disabled = recording;
		recordButton.Disabled = recording;

		// set progress bar value
		if (recording) progressbar.Value = (recordingTimer / (10f * (32f * (60f / (float)Manager.instance.bpm)))) * 2f;
	}

	public void OnBeginning()
	{
		if (recording)
		{
			StopRecording();
		}
		else
		{
			if (shouldRecord)
			{
				StartRecording();
			}
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
		voiceOver = audioEffectRecord.GetRecording();
    }
}
