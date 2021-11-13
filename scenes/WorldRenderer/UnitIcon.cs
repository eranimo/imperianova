using Godot;
using System;

public class UnitIcon : Panel {
	private TextureRect Icon;

	public override void _Ready() {
		this.Icon = (TextureRect) GetNode("MarginContainer/Icon");
	}

	public void SetIcon(string icon) {
		// load icon resource and replace texture
	}
}
