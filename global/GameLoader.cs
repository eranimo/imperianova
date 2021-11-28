using Godot;
using System;

public class GameLoader : Node {
	public enum GameLoaderState {
		NewGame,
		LoadGame,
		// quick start (new game with default settings)
		// continue (load last save)
	}
	public GameLoaderState State;

	public GameSetup.GameSettings GameSettings;

	public void NewGame(GameSetup.GameSettings gameSettings) {
		GD.PrintS("GameLoader NewGame");
		State = GameLoaderState.NewGame;
		GameSettings = gameSettings;
	}
}
