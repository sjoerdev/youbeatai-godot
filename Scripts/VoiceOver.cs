using Godot;
using System;

public partial class VoiceOver : Node
{
    AudioEffectRecord audioEffectRecord;
	AudioStreamPlayer2D audioPlayer;
	AudioStream currentVoiceOver = null;
	bool recording = false;
	
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
		// debug record keys
		if (Input.IsKeyPressed(Key.Left) && !recording) StartRecording();
		if (Input.IsKeyPressed(Key.Right) && recording) StopRecording();
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