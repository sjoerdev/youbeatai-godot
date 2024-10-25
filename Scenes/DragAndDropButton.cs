using Godot;
using System;

public partial class DragAndDropButton : Sprite2D
{
	[Export] int ring = 0;

	bool inside => IsPixelOpaque(GetLocalMousePosition());
	bool pressing = false;

	float timePressing = 0;

	public bool holding = false;

	public override void _Input(InputEvent inputEvent)
    {
		if (inputEvent is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.IsPressed()) pressing = true;

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

		if (pressing && inside && timePressing > 0.5f) Add();
    }

	private void Add()
	{
		GD.Print("add");
		Manager.instance.beatActives[ring, Manager.instance.currentBeat] = true;
	}

	private void Drop()
	{
		GD.Print("drop");
	}
}
