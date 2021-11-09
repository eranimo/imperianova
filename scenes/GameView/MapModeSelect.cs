using Godot;
using System;
using System.Collections.Generic;

public class MapModeSelect : OptionButton {
	private InputManager inputManager;

	public override void _Ready() {
		inputManager = GetNode<InputManager>("/root/InputManager");

		foreach (KeyValuePair<MapModes.MapMode, string> item in MapModes.mapModeTitles) {
			AddItem(item.Value, (int) item.Key);
		}
	}

	private void _on_MapModeSelect_item_selected(int index) {
		inputManager.ActiveMapMode.OnNext((MapModes.MapMode) index);
	}
}
