using Godot;
using System;

public partial class Pill 
	: Area2D
{
	[Signal] public delegate void PillTimedOutEventHandler();

	public void SetUp(Vector2 position)
	{
		Position = position;
	}

	public void StopTimer()
	{
		GetNode<Timer>("Timer").Stop();
	}

#region signals
	void OnTimerTimeout()
	{
		QueueFree();
		EmitSignal(SignalName.PillTimedOut);
	}
#endregion
}
