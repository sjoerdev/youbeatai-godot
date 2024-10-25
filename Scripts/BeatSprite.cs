using Godot;
using System;

public partial class BeatSprite : Sprite2D
{
	public int ringIndex;
	public int spriteIndex;

    public override void _Input(InputEvent inputEvent)
    {
		if (inputEvent is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.IsReleased() && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (IsPixelOpaque(GetLocalMousePosition()))
				{
					Manager.instance.beatActives[ringIndex, spriteIndex] = !Manager.instance.beatActives[ringIndex, spriteIndex];
				}
			}
		}
    }
}