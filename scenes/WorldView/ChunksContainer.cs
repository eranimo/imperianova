using Godot;
using Hex;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

public class ChunksContainer : Spatial {
	Dictionary<HexCell, MapChunk> cellChunks = new Dictionary<HexCell, MapChunk>();
	private HexGrid grid;

	public BehaviorSubject<HexCell> pressedCell = new BehaviorSubject<HexCell>(null);
	public BehaviorSubject<HexCell> hoveredCell = new BehaviorSubject<HexCell>(null);

	public void SetupChunks(HexGrid grid) {
		this.grid = grid;
		var chunkSize = new OffsetCoord(10, 10);
		for (var x = 0; x < grid.Size.Col; x += chunkSize.Col) {
			for (var y = 0; y < grid.Size.Row; y += chunkSize.Row) {
				var cell = this.grid.GetCell(new OffsetCoord(x, y));
				MapChunk chunk = new MapChunk(
					this,
					grid,
					new OffsetCoord(x, y),
					chunkSize
				);
				chunk.Name = $"Chunk ({x}, {y})";

				for (var cx = x; cx < x + chunkSize.Col; cx++) {
					for (var cy = y; cy < y + chunkSize.Row; cy++) {
						cellChunks[this.grid.GetCell(new OffsetCoord(cx, cy))] = chunk;
					}
				}
				AddChild(chunk);
			}
		}
	}

	public void RegenerateCell(HexCell cell) {
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
