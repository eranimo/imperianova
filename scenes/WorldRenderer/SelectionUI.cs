using Godot;
using System;

public class SelectionUI : Node2D {
	private bool dragging = false;
	private Vector2 drag_start = new Vector2(0, 0);
	private Nullable<Rect2> selection_rect = null;
	private RectangleShape2D selection_shape = new RectangleShape2D();
	private Godot.Collections.Array selected = new Godot.Collections.Array();

	public override void _Ready() {
		
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (@event.IsActionPressed("ui_select")) {
			dragging = true;
			drag_start = GetGlobalMousePosition();
		} else if (@event.IsActionReleased("ui_select") && dragging) {
			dragging = false;
			selection_rect = null;
			Update();
			var drag_end = GetGlobalMousePosition();
			selection_shape.Extents = (drag_end - drag_start) / 2;
			var space = GetWorld2d().DirectSpaceState;
			var query = new Physics2DShapeQueryParameters();
			query.SetShape(selection_shape);
			query.Transform = new Transform2D(0, (drag_end + drag_start) / 2);

			if (!Input.IsKeyPressed((int) KeyList.Shift)) {
				// TODO clear selected units
				foreach (var item in selected) {
					var unitIcon = (item as Godot.Collections.Dictionary)["collider"] as UnitIcon;
					unitIcon.SetSelected(false);
				}
			}
			selected = space.IntersectShape(query);
			foreach (var item in selected) {
				var unitIcon = (item as Godot.Collections.Dictionary)["collider"] as UnitIcon;
				GD.PrintS("Selected:", unitIcon);
				unitIcon.SetSelected(true);
			}
		}

		if (@event is InputEventMouseMotion) {
			if (dragging) {
				selection_rect = new Rect2(drag_start, GetGlobalMousePosition() - drag_start);
				Update();
			}
		}
	}

	public override void _Draw() {
		if (selection_rect.HasValue) {
			DrawRect(
				selection_rect.Value,
				new Color(1, 1, 1, 1),
				false,
				1,
				true
			);
		}
	}
}
