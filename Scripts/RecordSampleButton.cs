using Godot;
using System;

public partial class RecordSampleButton : Sprite2D
{
    [Export] int ring = 0;

    private bool inside => IsPixelOpaque(GetLocalMousePosition());
    private bool pressing = false;

    public AudioStream recordedAudio = null;

    private AudioEffectRecord audioEffectRecord;

    public override void _Ready()
    {
        var busIndex = AudioServer.GetBusIndex("Microphone");
        audioEffectRecord = (AudioEffectRecord)AudioServer.GetBusEffect(busIndex, 1);
		if (audioEffectRecord == null) GD.Print("no record effect found");
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            // On press
            if (mouseEvent.IsPressed())
            {
                pressing = true;
                if (inside) StartRecording();
            }

            // On release
            if (mouseEvent.IsReleased())
            {
                pressing = false;
                if (inside) StopRecording();
            }
        }
    }

    private void StartRecording()
    {
		Modulate = new Color(1, 0, 0, 1);
        audioEffectRecord.SetRecordingActive(true);
    }

    private void StopRecording()
    {
		Modulate = new Color(1, 1, 1, 1);
        audioEffectRecord.SetRecordingActive(false);
		var audioData = audioEffectRecord.GetRecording();
		recordedAudio = audioData;
    }
}