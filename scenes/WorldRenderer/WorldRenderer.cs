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
			GD.PrintS("Hex:", HexUtils.PixelToHex(point, 24));
		}
	}

	public void SetWorld() {
		this.Grid.Setup();
	}
}
