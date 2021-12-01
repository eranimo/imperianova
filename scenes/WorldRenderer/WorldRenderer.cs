using Godot;
using System;
using Hex;

public class WorldRenderer : Node2D {
	private WorldGrid Grid;
	private InputManager inputManager;

	public override void _Ready() {
		this.Grid = (WorldGrid) FindNode("WorldGrid");
		Grid.SetGridVisibility(true);

		inputManager = GetNode<InputManager>("/root/InputManager");
		inputManager.Connect("CameraZoom", this, nameof(_on_camera_zoom));
		inputManager.SelectedTile.Subscribe((Nullable<Hex.OffsetCoord> tile) => {
			if (!tile.HasValue) {
				Grid.SetSelectedHex(new Hex.OffsetCoord(-1, -1));
			} else {
				Grid.SetSelectedHex(tile.Value);
			}
		});
	}

	public Hex.OffsetCoord GetHexAtCursor() {
		var point = this.GetLocalMousePosition();
		var pointCorrected = point - new Vector2(HexUtils.HexWidth / 2, HexUtils.HexHeight / 2);
		return HexUtils.PixelToHexOffset(pointCorrected);
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("map_toggle_grid")) {
			Grid.SetGridVisibility(!Grid.IsGridVisible);
		}
	}

	private void _on_camera_zoom(float zoom) {
		// GD.PrintS("Zoom", zoom);
		Grid.SetZoom(zoom);
	}
}
