using Godot;
using System;

public class UnitIcon : KinematicBody2D {
	private Panel bg;
	private TextureRect Icon;

	public override void _Ready() {
		this.bg = (Panel) GetNode("BG");
		this.Icon = (TextureRect) GetNode("BG/MarginContainer/Icon");
	}

	public void SetIcon(string icon) {
		// load icon resource and replace texture
		Icon.Texture.ResourcePath = $"res://assets/icons/{icon}.svg";
	}

	public bool Selected = false;

	public void SetSelected(bool Selected) {
		this.Selected = Selected;
		GD.PrintS("Selected unit");
	}
}
