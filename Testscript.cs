using Godot;
using System;

public partial class Testscript : Sprite2D
{
	[Export]
	public PackedScene prefab;
	
	public override void _Ready()
	{
		Node2D instance = prefab.Instantiate<Node2D>();
		instance.Position = new Vector2(200, 150);
		AddChild(instance);
	}

	public override void _Process(double delta)
	{
		Rotation += 1 * (float)delta;
	}
}
