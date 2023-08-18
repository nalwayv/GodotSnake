using Godot;
using System;

public partial class Hud 
	: CanvasLayer
{
	[Signal] public delegate void StartGameEventHandler();
	Label _scoreLabel;
	Label _messageLabel;
	Button _startButton;

	public void UpdateScore(int score)
	{
		_scoreLabel.Text = score.ToString();
	}

	public async void ShowGameOver()
	{
		_scoreLabel.Hide();
		_messageLabel.Show();
		_messageLabel.Text = "GameOver";

		var timer = GetNode<Timer>("MessageTimer");
		timer.Start();

		// await for timer to finish
		await ToSignal(timer, Timer.SignalName.Timeout);

		_messageLabel.Text = "Restart";

		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

		_startButton.Show();
	}

#region signals
	void OnStartButtonPressed()
	{
		_startButton.Hide();
		_messageLabel.Hide();
		_scoreLabel.Show();

		EmitSignal(SignalName.StartGame);
	}
#endregion

#region override
	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("Score");
		_messageLabel = GetNode<Label>("Message");
		_startButton = GetNode<Button>("StartButton");

		_scoreLabel.Hide();
	}
#endregion
}
