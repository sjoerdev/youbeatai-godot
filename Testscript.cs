using Godot;
using System;

public partial class Testscript : Sprite2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Rotation += 1 * (float)delta;
	}
}
