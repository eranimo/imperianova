using Godot;
using System;

public class GameView : Node2D {
	Popup GameMenu;
	GameController Game;
	bool isMenuOpen;

	public override void _Ready() {
		GD.PrintS("[GameView] ready");
		GameMenu = (Popup) GetNode("GameMenu");
		Game = (GameController) GetNode("Game");

		GameMenu.Connect("hide", this, nameof(_on_menu_close));
		GameMenu.Connect("about_to_show", this, nameof(_on_menu_open));

		GameLoader gameLoader = GetNode<GameLoader>("/root/GameLoader");

		if (gameLoader.State == GameLoader.GameLoaderState.NewGame) {
			GameSetup.NewGame(Game);
		}
	}

	public override void _ExitTree() {
		if (isMenuOpen) {
			_on_menu_close();
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_exit")) {
			if (GameMenu.Visible) {
				CloseMenu();
			} else {
				OpenMenu();
			}
			GetTree().SetInputAsHandled();
		}
	}

	public void OpenMenu() {
		GameMenu.PopupCentered();
	}

	public void CloseMenu() {
		GameMenu.Hide();
	}

	private void _on_menu_close() {
		GetTree().Paused = false;
		isMenuOpen = false;
	}

	private void _on_menu_open() {
		GetTree().Paused = true;
		isMenuOpen = true;
	}
}
