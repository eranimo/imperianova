using Godot;
using Hex;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Reactive.Subjects;

public struct ChunkThreadInfo {
	public int ThreadIndex;
	public OffsetCoord Coord;
}

public class ChunksContainer : Spatial {
	Dictionary<HexCell, MapChunk> cellChunks = new Dictionary<HexCell, MapChunk>();
	private HexGrid grid;

	public BehaviorSubject<HexCell> pressedCell = new BehaviorSubject<HexCell>(null);
	public BehaviorSubject<HexCell> hoveredCell = new BehaviorSubject<HexCell>(null);

	public static readonly OffsetCoord chunkSize = new OffsetCoord(10, 10);

	public void SetupChunks(HexGrid grid) {
		this.grid = grid;
		var numChunks = (grid.Size.Col / chunkSize.Col) * (grid.Size.Row / chunkSize.Row);
		var doneEvent = new CountdownEvent(numChunks);
		int i = 0;
		var mapChunksData = new List<MapChunkData>();
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (var x = 0; x < grid.Size.Col; x += chunkSize.Col) {
			for (var y = 0; y < grid.Size.Row; y += chunkSize.Row) {
				var chunkCoord = new OffsetCoord(x, y);
				var mapChunkData = new MapChunkData(grid, chunkCoord, doneEvent);
				mapChunksData.Add(mapChunkData);
				ThreadPool.QueueUserWorkItem(mapChunkData.ThreadPoolCallback);
				i++;
			}
		}
		doneEvent.Wait();
		GD.PrintS($"Chunks generate: {watch.ElapsedMilliseconds}ms");

		watch = System.Diagnostics.Stopwatch.StartNew();

		foreach (MapChunkData mapChunkData in mapChunksData) {
			var chunk = new MapChunk(this, grid, mapChunkData);
			var chunkCoord = mapChunkData.coord;
			for (var cx = chunkCoord.Col; cx < chunkCoord.Col + chunkSize.Col; cx++) {
				for (var cy = chunkCoord.Row; cy < chunkCoord.Row + chunkSize.Row; cy++) {
					cellChunks[this.grid.GetCell(new OffsetCoord(cx, cy))] = chunk;
				}
			}
			AddChild(chunk);
		}

		GD.PrintS($"Chunks render: {watch.ElapsedMilliseconds}ms");
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
