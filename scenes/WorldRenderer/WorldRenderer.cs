using Godot;
using System;
using Hex;

public class WorldRenderer : Node2D {
	private WorldGrid Grid;

	public override void _Ready() {
		this.Grid = (WorldGrid) FindNode("WorldGrid");
		this.SetWorld();
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion) {
			var point = (@event as InputEventMouseMotion).Position;
			var pointCorrected = point - new Vector2(HexUtils.HexWidth / 2, HexUtils.HexHeight / 2);
			var hex = HexUtils.PixelToHexOffset(pointCorrected);
			GD.PrintS("Hex:", hex, hex.AsVector());
			this.Grid.setHighlight(hex);

		}
	}

	public void SetWorld() {
		this.Grid.SetupGrid(10, 10);
		this.Grid.Setup();
	}
}
