using Godot;
using System;

public class UnitIcon : Panel {
	private TextureRect icon;

	public override void _Ready() {
		this.icon = (TextureRect) GetNode("MarginContainer/Icon");
	}

	
}
