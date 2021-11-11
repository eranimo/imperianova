using Godot;
using System;

public class WorldGridContainer : Control {
	private WorldRenderer worldRenderer;
	private WorldGrid worldGrid;
	private InputManager inputManager;

	public override void _Ready() {
		this.worldRenderer = (WorldRenderer) GetParent();
		this.worldGrid = (WorldGrid) GetNode("WorldGrid");
		this.inputManager = GetNode<InputManager>("/root/InputManager");
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion) {
			var hex = worldRenderer.GetHexAtCursor();
			this.worldGrid.SetHighlightedHex(hex);
			if (worldRenderer.world.IsValidTile(hex)) {
				inputManager.EmitSignal("TileHovered", hex.AsVector());
			} else {
				inputManager.EmitSignal("TileHovered", new Vector2(-1, -1));
			}
		} else if (@event.IsActionPressed("ui_select")) {
			var hex = worldRenderer.GetHexAtCursor();
			GD.PrintS("Pressed hex", hex);
			if (worldRenderer.world.IsValidTile(hex)) {
				var tile = worldRenderer.world.GetTile(hex);
				if (inputManager.SelectedTile.Value == tile) {
					inputManager.SelectedTile.OnNext(null);
				} else {
					inputManager.SelectedTile.OnNext(tile);
				}
			}
		}
	}
}
