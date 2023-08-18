using Godot;
using System;

public partial class Body 
	: Area2D
{
	public Vector2 LastPosition {get; private set;}

	public void SetUp(Vector2 position)
	{
		Position = position;
	}

	public void MoveTo(Vector2 newPos)
	{
		LastPosition = Position;
		Position = newPos;
	}
}
