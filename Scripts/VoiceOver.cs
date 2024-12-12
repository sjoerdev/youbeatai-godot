using Godot;
using System;

public partial class VoiceOver : Node
{
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
		// create audioplayer
		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		// setup record effect
        audioEffectRecord = (AudioEffectRecord)AudioServer.GetBusEffect(AudioServer.GetBusIndex("Microphone"), 1);
    }

    public override void _Process(double delta)
	{
		// update recording timer
		if (recording) recordingTimer += (float)delta;
		else recordingTimer = 0;


		// debug record keys
		if (Input.IsKeyPressed(Key.Left) && !recording) StartRecording();
		if (Input.IsKeyPressed(Key.Right) && recording) SetCurrentLayerVoiceOver(StopRecording());

		// set progress bar value
		if (recording) textureProgressBar.Value = recordingTimer;
		else if (GetCurrentLayerVoiceOver() != null) textureProgressBar.Value = GetCurrentLayerVoiceOver().GetLength();
	}

    private void StartRecording()
    {
		recording = true;
        audioEffectRecord.SetRecordingActive(true);
		GD.Print("recording started");
    }

    private AudioStream StopRecording()
    {
        audioEffectRecord.SetRecordingActive(false);
		GD.Print("recording stopped");
		recording = false;
		return audioEffectRecord.GetRecording();
    }
}