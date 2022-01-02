using Godot;
using System;

public class Camera : Godot.Camera {
	const float PanningSpeed = 1.0f;
	const float ZoomSpeed = 25.0f;
	const float MoveSpeed = 25.0f;

	bool panning = false;

	public override void _Ready() {
		
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("view_pan_mouse")) {
			panning = true;
		} else if (@event.IsActionReleased("view_pan_mouse")) {
			panning = false;
		}

		if (@event is InputEventMouseMotion && panning) {
			var mouseMotion = (InputEventMouseMotion) @event;
			Translation -= new Vector3(mouseMotion.Relative.x, 0, mouseMotion.Relative.y);
		}

		if (@event.IsAction("ui_left")) {
			Translation -= new Vector3(MoveSpeed, 0f, 0f);
		} else if (@event.IsAction("ui_right")) {
			Translation += new Vector3(MoveSpeed, 0f, 0f);
		} else if (@event.IsAction("ui_up")) {
			Translation -= new Vector3(0f, 0f, MoveSpeed);
		} else if (@event.IsAction("ui_down")) {
			Translation += new Vector3(0f, 0f, MoveSpeed);
		}

		if (@event.IsActionReleased("view_zoom_in")) {
			Translation -= new Vector3(0, ZoomSpeed, 0);
		} else if (@event.IsActionReleased("view_zoom_out")) {
			Translation += new Vector3(0, ZoomSpeed, 0);
		}
		// translation.y = clamp(translation.y, 1, 500)
		Translation = new Vector3(
			Translation.x,
			(float) Math.Min(Math.Max(Translation.y, 1), 500),
			Translation.z
		);
	}
}
