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
            // On Mouse Down
            if (mouseEvent.IsPressed() && inside)
            {
                pressing = !pressing;
                if (pressing) StartRecording();
                else StopRecording();
            }
        }
    }

    void SetVolume(float value)
    {
        float db = Mathf.LinearToDb(value);
        Manager.instance.firstAudioPlayer.VolumeDb = db;
        Manager.instance.secondAudioPlayer.VolumeDb = db;
        Manager.instance.thirdAudioPlayer.VolumeDb = db;
        Manager.instance.fourthAudioPlayer.VolumeDb = db;
    }

    private void StartRecording()
    {
        SetVolume(0.25f);
		Modulate = new Color(1, 0, 0, 1);
        audioEffectRecord.SetRecordingActive(true);
    }

    private void StopRecording()
    {
        SetVolume(1);
		Modulate = new Color(1, 1, 1, 1);
        audioEffectRecord.SetRecordingActive(false);
		var audioData = audioEffectRecord.GetRecording();
		recordedAudio = audioData;

        if (ring == 0)
        {
            var manager = Manager.instance;
            manager.firstAudioPlayer.Stop();
            var istoggleon = manager.recordSampleCheckButton0.ButtonPressed;
            manager.audioFilesToUse[ring] = istoggleon ? manager.recordSampleButton0.recordedAudio : manager.mainAudioFiles[ring];
            manager.firstAudioPlayer.Stream = manager.audioFilesToUse[ring];
        }
        if (ring == 1)
        {
            var manager = Manager.instance;
            manager.secondAudioPlayer.Stop();
            var istoggleon = manager.recordSampleCheckButton1.ButtonPressed;
            manager.audioFilesToUse[ring] = istoggleon ? manager.recordSampleButton1.recordedAudio : manager.mainAudioFiles[ring];
            manager.secondAudioPlayer.Stream = manager.audioFilesToUse[ring];
        }
        if (ring == 2)
        {
            var manager = Manager.instance;
            manager.thirdAudioPlayer.Stop();
            var istoggleon = manager.recordSampleCheckButton2.ButtonPressed;
            manager.audioFilesToUse[ring] = istoggleon ? manager.recordSampleButton2.recordedAudio : manager.mainAudioFiles[ring];
            manager.thirdAudioPlayer.Stream = manager.audioFilesToUse[ring];
        }
        if (ring == 3)
        {
            var manager = Manager.instance;
            manager.fourthAudioPlayer.Stop();
            var istoggleon = manager.recordSampleCheckButton3.ButtonPressed;
            manager.audioFilesToUse[ring] = istoggleon ? manager.recordSampleButton3.recordedAudio : manager.mainAudioFiles[ring];
            manager.fourthAudioPlayer.Stream = manager.audioFilesToUse[ring];
        }
    }
}