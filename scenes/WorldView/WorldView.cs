using Godot;
using System;
using Hex;
using System.Collections.Generic;

public class HexCell {
	public OffsetCoord Position;
	public HexGrid Grid;

	public double Height;
	public Color color;

	public HexCell(OffsetCoord position) {
		Position = position;
	}

	public HexCell GetNeighbor(Direction dir) {
		var pos = HexUtils.GetNeighbor(this.Position, dir);
		return Grid.GetCell(pos);
	}
}

public class HexGrid {
	public OffsetCoord Size;
	private Dictionary<OffsetCoord, HexCell> cells = new Dictionary<OffsetCoord, HexCell>();

	public HexGrid(OffsetCoord size) {
		Size = size;
	}

	public void AddCell(HexCell cell) {
		cells.Add(cell.Position, cell);
		cell.Grid = this;
	}

	public HexCell GetCell(OffsetCoord pos) {
		try {
			return cells[pos];
		} catch (KeyNotFoundException) {
			return null;
		}
	}
}

public class WorldView : Spatial {
    private HexGrid grid;

    public override void _Ready() {
		var chunks = GetNode("Chunks");

		this.grid = new HexGrid(new OffsetCoord(100, 100));
		var heightNoise = new GameWorld.WorldNoise(grid.Size.Col, grid.Size.Row, 123);
		for (var x = 0; x < grid.Size.Col; x++) {
			for (var y = 0; y < grid.Size.Row; y++) {
				var pos = new OffsetCoord(x, y);
				var cell = new HexCell(pos);
				cell.Height = (heightNoise.Get(x, y) / 255f) * 50;
				if (cell.Height < 25) {
					cell.color = new Color("#0356fc");
				} else {
					cell.color = new Color("#208a0e");
				}
				grid.AddCell(cell);
			}
		}

		var chunkSize = new OffsetCoord(10, 10);
		for (var x = 0; x < grid.Size.Col; x += chunkSize.Col) {
			for (var y = 0; y < grid.Size.Row; y += chunkSize.Row) {
				MapChunk chunk = new MapChunk(
					grid,
					new OffsetCoord(x, y),
					chunkSize
				);
				chunks.AddChild(chunk);
			}
		}
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
					var cell = grid.GetCell(hex);
					GD.Print("\n");
					GD.PrintS(hex, cell);
					if (cell != null) {
						GD.PrintS("Height:", cell.Height);
						GD.PrintT(new string[] {
							$"SE: {cell.GetNeighbor(Hex.Direction.SE)?.Height}",
							$"NE: {cell.GetNeighbor(Hex.Direction.NE)?.Height}",
							$"N: {cell.GetNeighbor(Hex.Direction.N)?.Height}",
							$"NW: {cell.GetNeighbor(Hex.Direction.NW)?.Height}",
							$"SW: {cell.GetNeighbor(Hex.Direction.SW)?.Height}",
							$"S: {cell.GetNeighbor(Hex.Direction.S)?.Height}",
						});
					}
				}
			}
		}
	}
}
