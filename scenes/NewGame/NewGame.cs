using GameWorld;
using Godot;
using System;

public class NewGame : Control {
	GameSetup.GameSettings gameSettings = new GameSetup.GameSettings();

	SpinBox WorldSeed;
	OptionButton WorldSize;
	SpinBox WorldSealevel;

	public override void _Ready() {
		WorldSeed = (SpinBox) FindNode("WorldSeed");
		WorldSize = (OptionButton) FindNode("WorldSize");
		WorldSealevel = (SpinBox) FindNode("WorldSealevel");

		WorldSeed.Connect("value_changed", this, nameof(_on_WorldSeed_changed));
		WorldSeed.Value = gameSettings.WorldOptions.Seed;

		WorldSize.Connect("item_selected", this, nameof(_on_WorldSize_changed));
		WorldSize.Selected = (int) gameSettings.WorldOptions.Size;

		WorldSealevel.Connect("value_changed", this, nameof(_on_WorldSealevel_changed));
		WorldSealevel.Value = gameSettings.WorldOptions.Sealevel;
	}

	private void _on_WorldSeed_changed(float value) {
		gameSettings.WorldOptions.Seed = (int) value;
	}

	private void _on_WorldSize_changed(int index) {
		gameSettings.WorldOptions.Size = (WorldSize) index;
	}

	private void _on_WorldSealevel_changed(float value) {
		gameSettings.WorldOptions.Sealevel = (int) value;
	}
	
	private void _on_StartGame_pressed() {
		// load to GameView scene
		// find GameController and set GameSettings
		// unload new game scene
		// change scene to GameView scene
	}
}
