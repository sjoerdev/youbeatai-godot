using Godot;
using System;

public partial class MicrophoneCapture : Node
{
	[Export] string busName = "Microphone";
	
    private AudioEffectCapture audioEffectCapture;
    private AudioStreamPlayer audioStreamPlayer;

    public override void _Ready()
    {
        audioStreamPlayer = new AudioStreamPlayer();
        AddChild(audioStreamPlayer);

        audioStreamPlayer.Stream = new AudioStreamMicrophone();

        audioStreamPlayer.Bus = busName;
        audioEffectCapture = (AudioEffectCapture)AudioServer.GetBusEffect(AudioServer.GetBusIndex(busName), 0);

        audioStreamPlayer.Play();
    }

    public override void _Process(double delta)
    {
		var frames = audioEffectCapture.GetFramesAvailable();
		if (frames > 0)
		{
			var audioData = audioEffectCapture.GetBuffer(frames);
			var volume = GetVolumeFromAudioData(audioData);
			GD.Print($"Microphone Volume: {volume}");
		}
    }

    private float GetVolumeFromAudioData(Vector2[] audioData)
    {
        float sum = 0f;
        foreach (Vector2 sample in audioData)
        {
            float averageSample = (Mathf.Abs(sample.X) + Mathf.Abs(sample.Y)) / 2;
            sum += averageSample;
        }
        return sum / audioData.Length;
    }
}