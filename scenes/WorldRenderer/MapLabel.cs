using Godot;
using System;

public class MapLabel : Control {
	private string _label;
	public string Label {
		get => _label;
		set => _setLabel(value);
	}

	private void _setLabel(string value) {
		_label = value;
		RecalculatePosition();
	}

	public override void _Ready() {
		RecalculatePosition();
	}

	private void RecalculatePosition() {
		Control container = (Control) GetNode("./Container");
		Vector2 pos = this.RectPosition;
		this.RectMinSize = new Vector2(container.GetRect().Size.x, 25);
		this.RectSize = this.RectMinSize;
		this.RectPosition = new Vector2(pos.x - this.RectSize.x / 2, pos.y - this.RectSize.y / 2);
	}

	// public override void _Input(InputEvent @event) {
	// 	if (@event.IsActionPressed("ui_select")) {
	// 		GD.PrintS("Click");
	// 		GD.PrintS("Pressed map label");
	// 		GetTree().SetInputAsHandled();
	// 	}
	// }

	public void UpdatePosition(Vector2 position) {
		SetPosition(position);
		RecalculatePosition();
	}

	private void _on_MapLabel_pressed() {
		GD.PrintS("Pressed map label");
		GetTree().SetInputAsHandled();
	}
}
