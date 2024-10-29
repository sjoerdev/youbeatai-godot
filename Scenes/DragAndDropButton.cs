using Godot;
using System;

public partial class DragAndDropButton : Sprite2D
{
	[Export] int ring = 0;

	bool inside => IsPixelOpaque(GetLocalMousePosition());
	bool pressing = false;

	float timePressing = 0;

	public bool holding = false;

	bool holdingforthis = false;

	public override void _Input(InputEvent inputEvent)
    {
		if (inputEvent is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.IsPressed())
				{
					if (IsPixelOpaque(GetLocalMousePosition())) holdingforthis = true;
					else holdingforthis = false;
					pressing = true;
				} 

				if (mouseEvent.IsReleased() && !inside)
				{
					Drop();
					pressing = false;
				}

				if (mouseEvent.IsReleased() && inside)
				{
					Add();
					pressing = false;
				}
			}
		}
    }

    public override void _Process(double delta)
    {
		holding = pressing && !inside;

		Manager.instance.dragginganddropping = holding;

		if (pressing) timePressing += (float)delta;
		else timePressing = 0;

		if (pressing && inside && timePressing > 0.5f && !Manager.instance.beatActives[ring, Manager.instance.currentBeat]) Add();

		bool hover = IsPixelOpaque(GetLocalMousePosition());
		if (hover) Modulate = Manager.instance.colors[ring];
		else Modulate = Manager.instance.colors[ring] / 2;

		if (holding && holdingforthis) Manager.instance.holdingforring = ring;
    }

	private void Add()
	{
		GD.Print("add");
		Manager.instance.beatActives[ring, Manager.instance.currentBeat] = true;

		if (ring == 0) Manager.instance.firstAudioPlayer.Play();
		if (ring == 1) Manager.instance.secondAudioPlayer.Play();
		if (ring == 2) Manager.instance.thirdAudioPlayer.Play();
		if (ring == 3) Manager.instance.fourthAudioPlayer.Play();
	}

	private void Drop()
	{
		GD.Print("drop");
	}
}
