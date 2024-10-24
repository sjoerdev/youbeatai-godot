using Godot;
using System;

public partial class ReverbDelayManager : Node
{
	[Export] Button reverbButton;
	[Export] Button delayButton;

	[Export] Sprite2D reverbSprite;
	[Export] Sprite2D delaySprite;

	AudioEffectReverb reverbEffect;
    AudioEffectDelay delayEffect;

    private int currentReverbLevel = 0;
    private int currentDelayLevel = 0;
    private float[] levels = { 0.0f, 0.5f, 1.0f };

    public override void _Ready()
    {
        reverbButton.Pressed += OnReverbButtonPressed;
        delayButton.Pressed += OnDelayButtonPressed;
        reverbEffect = new AudioEffectReverb();
        delayEffect = new AudioEffectDelay();
        AudioServer.AddBusEffect(AudioServer.GetBusIndex("Master"), reverbEffect);
        AudioServer.AddBusEffect(AudioServer.GetBusIndex("Master"), delayEffect);
        SetReverbLevel(levels[currentReverbLevel]);
        SetDelayLevel(levels[currentDelayLevel]);
    }

    private void OnReverbButtonPressed()
    {
        currentReverbLevel = (currentReverbLevel + 1) % levels.Length;
        SetReverbLevel(levels[currentReverbLevel]);
    }

	private void OnDelayButtonPressed()
    {
        currentDelayLevel = (currentDelayLevel + 1) % levels.Length;
        SetDelayLevel(levels[currentDelayLevel]);
    }

    private void SetReverbLevel(float level)
    {
        reverbEffect.RoomSize = level;
		reverbSprite.Scale = Vector2.One * (level / 2 + 0.1f);
    }

    private void SetDelayLevel(float level)
    {
        delayEffect.Dry = 1 - level;
		delaySprite.Scale = Vector2.One * (level / 2 + 0.1f);
    }
}
