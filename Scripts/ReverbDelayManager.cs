using Godot;
using System;

public partial class ReverbDelayManager : Node
{
    public static ReverbDelayManager instance = null;

	[Export] public Button reverbButton;
	[Export] public Button delayButton;

	[Export] Sprite2D reverbSprite;
	[Export] Sprite2D delaySprite;

	AudioEffectReverb reverbEffect;
    AudioEffectDelay delayEffect;

    public int currentReverbLevel = 0;
    public int currentDelayLevel = 0;
    
    private float[] reverbLevels = { 0.0f, 0.5f, 1.0f };
    private float[] delayLevels = { 0.0f, 0.3f, 0.6f };

    public override void _Ready()
    {
        instance ??= this;
        reverbButton.Pressed += OnReverbButtonPressed;
        delayButton.Pressed += OnDelayButtonPressed;
        reverbEffect = new AudioEffectReverb();
        delayEffect = new AudioEffectDelay();
        AudioServer.AddBusEffect(AudioServer.GetBusIndex("Master"), reverbEffect);
        AudioServer.AddBusEffect(AudioServer.GetBusIndex("Master"), delayEffect);
        SetReverbLevel(reverbLevels[currentReverbLevel]);
        SetDelayLevel(delayLevels[currentDelayLevel]);
    }

    private void OnReverbButtonPressed()
    {
        currentReverbLevel = (currentReverbLevel + 1) % reverbLevels.Length;
        SetReverbLevel(reverbLevels[currentReverbLevel]);
    }

	private void OnDelayButtonPressed()
    {
        currentDelayLevel = (currentDelayLevel + 1) % delayLevels.Length;
        SetDelayLevel(delayLevels[currentDelayLevel]);
    }

    private void SetReverbLevel(float level)
    {
        reverbEffect.RoomSize = level;
		reverbSprite.Scale = Vector2.One * (level / 2 + 0.1f);
    }

    private void SetDelayLevel(float level)
    {
        delayEffect.Tap1Active = true;
        delayEffect.Tap2Active = false;
        delayEffect.Tap1DelayMs = level * 1000f;
		delaySprite.Scale = Vector2.One * (level / 2 + 0.1f);
    }
}
