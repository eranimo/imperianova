using Godot;
using System;
using Hex;
using System.Collections.Generic;

public class HexCell {
	public OffsetCoord Position;
	public HexGrid Grid;

	public double Height;
	public double WaterLevel;
	public Color Color;
	public Direction? IncomingRiver = null;
	public Direction? OutgoingRiver = null;

	public HexCell(OffsetCoord position) {
		Position = position;
	}

	public Vector3 Center {
		get {
			var pos = HexUtils.HexToPixelCenter(Position);
			return new Vector3(pos.x, (float) Height, pos.y);
		}
	}

	public Vector3 WaterCenter {
		get {
			var pos = HexUtils.HexToPixelCenter(Position);
			return new Vector3(pos.x, (float) WaterLevel, pos.y);
		}
	}

	public HexCell GetNeighbor(Direction dir) {
		var pos = HexUtils.GetNeighbor(this.Position, dir);
		return Grid.GetCell(pos);
	}

	public bool IsUnderwater {
		get {
			return WaterLevel > Height;
		}
	}

	public bool HasRiver {
		get {
			return IncomingRiver != null || OutgoingRiver != null;
		}
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
	Dictionary<HexCell, MapChunk> cellChunks = new Dictionary<HexCell, MapChunk>();

	public override void _Ready() {
		var chunks = GetNode("Chunks");
		var watch = System.Diagnostics.Stopwatch.StartNew();

		this.grid = new HexGrid(new OffsetCoord(100, 100));

		var heightNoise = new GameWorld.WorldNoise(grid.Size.Col, grid.Size.Row, 123);
		for (var x = 0; x < grid.Size.Col; x++) {
			for (var y = 0; y < grid.Size.Row; y++) {
				var pos = new OffsetCoord(x, y);
				var cell = new HexCell(pos);
				cell.Height = (heightNoise.Get(x, y) / 255f) * 100;
				if (cell.Height > 75) {
					cell.Height = 60;
					cell.Color = new Color("#619960");
				} else if (cell.Height > 50) {
					cell.Height = 20 + (Math.Round((cell.Height - 50) / 5) * 5);
					cell.Color = new Color("#208a0e");
				} else if (cell.Height > 48) {
					cell.Height = 15;
					cell.Color = new Color("#ebe571");
				} else if (cell.Height > 25) {
					cell.Height = 10;
					cell.Color = new Color("#0356fc");
				} else {
					cell.Height = 5;
					cell.Color = new Color("#003bb0");
				}
				cell.WaterLevel = 12.5;
				grid.AddCell(cell);
			}
		}

		var chunkSize = new OffsetCoord(10, 10);
		for (var x = 0; x < grid.Size.Col; x += chunkSize.Col) {
			for (var y = 0; y < grid.Size.Row; y += chunkSize.Row) {
				var cell = this.grid.GetCell(new OffsetCoord(x, y));
				MapChunk chunk = new MapChunk(
					grid,
					new OffsetCoord(x, y),
					chunkSize
				);
				for (var cx = x; cx < x + chunkSize.Col; cx++) {
					for (var cy = y; cy < y + chunkSize.Row; cy++) {
						cellChunks[this.grid.GetCell(new OffsetCoord(cx, cy))] = chunk;
					}
				}
				chunks.AddChild(chunk);
			}
		}
		GD.PrintS($"WorldView init: {watch.ElapsedMilliseconds}ms");
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
					cell.Height = 90;
					cellChunks[cell].Generate();
					foreach(OffsetCoord c in HexUtils.GetRing(cell.Position)) {
						MapChunk cellChunk;
						var ring_cell = grid.GetCell(c);
						if (ring_cell != null && cellChunks.TryGetValue(ring_cell, out cellChunk)) {
							cellChunk.Generate();
						}
					}
				}
			}
		}
	}
}
