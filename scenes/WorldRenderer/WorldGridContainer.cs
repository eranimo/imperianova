using Godot;
using System;

public class WorldGridContainer : Control {
	private WorldRenderer worldRenderer;
	private WorldGrid worldGrid;
	private InputManager inputManager;
	private GameController gameController;

	public override void _Ready() {
		this.worldRenderer = (WorldRenderer) GetParent();
		this.worldGrid = (WorldGrid) GetNode("WorldGrid");
		this.inputManager = GetNode<InputManager>("/root/InputManager");
		this.gameController = (GameController) GetTree().CurrentScene.FindNode("Game");
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion) {
			var hex = worldRenderer.GetHexAtCursor();
			this.worldGrid.SetHighlightedHex(hex);
			if (gameController.world.IsValidTile(hex)) {
				inputManager.EmitSignal("TileHovered", hex.AsVector());
			} else {
				inputManager.EmitSignal("TileHovered", new Vector2(-1, -1));
			}
		} else if (@event.IsActionPressed("ui_select")) {
			var hex = worldRenderer.GetHexAtCursor();
			GD.PrintS("Pressed hex", hex);
			if (gameController.world.IsValidTile(hex)) {
				var isValid = gameController.world.IsValidTile(hex);
				if (inputManager.SelectedTile.Value.HasValue && inputManager.SelectedTile.Value.Value.Equals(hex)) {
					inputManager.SelectedTile.OnNext(null);
				} else if (isValid) {
					inputManager.SelectedTile.OnNext(hex);
				}
			}
		}
	}
}
