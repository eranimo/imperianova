using Godot;
using System;

public class WorldView : Spatial {
	public override void _Ready() {
		var chunks = GetNode("Chunks");

		MapChunk chunk = new MapChunk(10, 10);
		chunks.AddChild(chunk);
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_select") && @event is InputEventMouseButton eventMouseButton) {
			var spaceState = GetWorld().DirectSpaceState;
			var camera = GetViewport().GetCamera();
			var from = camera.ProjectRayOrigin(eventMouseButton.Position);
			var to = from + camera.ProjectRayNormal(eventMouseButton.Position) * 1000;
			var result = spaceState.IntersectRay(from, to);
			if (result.Contains("collider")) {
				var collider = result["collider"];
				var position = (Vector3) result["position"];
				if (collider is MapChunk) {
					var chunk = (MapChunk) collider;
					var o = chunk.GlobalTransform.origin;
					var hexPosition = new Vector2(o.x, o.z) + new Vector2(position.x, position.z);
					var hex = Hex.HexUtils.PixelToHexOffset(hexPosition - Hex.HexUtils.HexCenter);
					GD.PrintS(hexPosition, hex);
				}
			}
		}
	}
}
