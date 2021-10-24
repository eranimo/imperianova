using Godot;
using System;

public class GameHeader : PanelContainer {
	private Godot.Button playButton;
	private Godot.Button changeSpeed;
	private Godot.Label dateDisplay;
	private Godot.Button menu;
	private Game game;

	public override void _Ready() {
		this.playButton = GetNode<Godot.Button>("Content/Grid/LeftColumn/PlayButton");
		this.dateDisplay = GetNode<Godot.Label>("Content/Grid/LeftColumn/Date");
		this.changeSpeed = GetNode<Godot.Button>("Content/Grid/LeftColumn/ChangeSpeed");
		this.menu = GetNode<Godot.Button>("Content/Grid/RightColumn/Menu");
		this.game = (Game) GetTree().CurrentScene.FindNode("Game");

		this.game.date.Subscribe((int date) => {
			GD.PrintS("New Date", date);
			this.dateDisplay.Text = date.ToString();
		});

		this.game.playState.Subscribe((bool isPlaying) => {
			this.playButton.Text = isPlaying ? "Pause" : "Play"; 
		});

		this.game.speed.Subscribe((Speed speed) => {
			switch (speed) {
				case Speed.Slow:
					this.changeSpeed.Text = "Slow";
					return;
				case Speed.Normal:
					this.changeSpeed.Text = "Normal";
					return;
				case Speed.Fast:
					this.changeSpeed.Text = "Fast";
					return;
			}
		});

		this.playButton.Connect("pressed", this, "_play_pressed");
		this.changeSpeed.Connect("pressed", this, "_speed_pressed");
	}

	private void _play_pressed() {
		this.game.TogglePlay();
	}

	private void _speed_pressed() {
		this.game.ToggleSpeed();
	}
}
