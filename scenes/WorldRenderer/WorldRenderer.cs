using Godot;
using System;
using Hex;

public class WorldRenderer : Node2D {
	private WorldGrid Grid;
	private InputManager inputManager;
	public GameWorld.World world;

	public override void _Ready() {
		this.Grid = (WorldGrid) FindNode("WorldGrid");

		inputManager = GetNode<InputManager>("/root/InputManager");
		inputManager.Connect("CameraZoom", this, nameof(_on_camera_zoom));
		inputManager.SelectedTile.Subscribe((GameWorld.Tile tile) => {
			if (tile == null) {
				Grid.SetSelectedHex(new Hex.OffsetCoord(-1, -1));
			} else {
				Grid.SetSelectedHex(tile.position);
			}
		});
	}

	private Hex.OffsetCoord GetHexAtCursor() {
		var point = this.GetLocalMousePosition();
		var pointCorrected = point - new Vector2(HexUtils.HexWidth / 2, HexUtils.HexHeight / 2);
		return HexUtils.PixelToHexOffset(pointCorrected);
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion) {
			var hex = GetHexAtCursor();
			Grid.SetHighlightedHex(hex);
			if (world.IsValidTile(hex)) {
				inputManager.EmitSignal("TileHovered", hex.AsVector());
			} else {
				inputManager.EmitSignal("TileHovered", new Vector2(-1, -1));
			}
		} else if (@event.IsActionPressed("ui_select")) {
			var hex = GetHexAtCursor();
			GD.PrintS("Pressed hex", hex);
			if (world.IsValidTile(hex)) {
				var tile = world.GetTile(hex);
				if (inputManager.SelectedTile.Value == tile) {
					inputManager.SelectedTile.OnNext(null);
				} else {
					inputManager.SelectedTile.OnNext(tile);
				}
			}
		} else if (@event.IsActionPressed("map_toggle_grid")) {
			Grid.SetGridVisibility(!Grid.IsGridVisible);
		}
	}

	public void RenderWorld(GameWorld.World world) {
		this.world = world;
		Grid.Render(world);
	}

	private void _on_camera_zoom(float zoom) {
		if (Grid.IsGridVisible) {
			Grid.SetGridVisibility(zoom < 1.0);
		}
	}
}
