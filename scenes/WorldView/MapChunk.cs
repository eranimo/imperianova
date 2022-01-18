using Godot;
using Godot.Collections;
using System;
using Hex;
using LibNoise.Primitive;


public struct EdgeVertices {
	public Vector3 v1, v2, v3, v4, v5, v6, v7, v8, v9;

	public EdgeVertices(Vector3 corner1, Vector3 corner2) {
		v1 = corner1;
		v2 = corner1.LinearInterpolate(corner2, 1f / 8f);
		v3 = corner1.LinearInterpolate(corner2, 2f / 8f);
		v4 = corner1.LinearInterpolate(corner2, 3f / 8f);
		v5 = corner1.LinearInterpolate(corner2, 4f / 8f);
		v6 = corner1.LinearInterpolate(corner2, 5f / 8f);
		v7 = corner1.LinearInterpolate(corner2, 6f / 8f);
		v8 = corner1.LinearInterpolate(corner2, 7f / 8f);
		v9 = corner2;
	}
}

public class MapChunk : Spatial {
	private readonly ChunksContainer chunks;
	private readonly HexGrid hexGrid;
    private readonly MapChunkData mapChunkData;
    private readonly OffsetCoord firstHex;
	private readonly OffsetCoord chunkSize;
	private Random rng;

	private SimplexPerlin noise;
	private HexMesh terrain;
	private HexMesh water;
	private HexMesh waterShore;
	private HexMesh rivers;


	public MapChunk(
		ChunksContainer chunks,
		HexGrid hexGrid,
		MapChunkData mapChunkData
	) {
		this.chunks = chunks;
		this.hexGrid = hexGrid;
        this.mapChunkData = mapChunkData;
        rng = new Random();

		var terrainShader = (ShaderMaterial) ResourceLoader.Load("res://scenes/WorldView/materials/TerrainShader.tres");
		var waterShader = (ShaderMaterial) ResourceLoader.Load("res://scenes/WorldView/materials/WaterShader.tres");
		var waterShoreShader = (ShaderMaterial) ResourceLoader.Load("res://scenes/WorldView/materials/WaterShoreShader.tres");
		var riverShader = (ShaderMaterial) ResourceLoader.Load("res://scenes/WorldView/materials/RiverShader.tres");

		terrain = new HexMesh("Terrain", mapChunkData.terrain, terrainShader, true);
		water = new HexMesh("Water", mapChunkData.water, waterShader);
		waterShore = new HexMesh("WaterShore", mapChunkData.waterShore, waterShoreShader, false, true);
		rivers = new HexMesh("Rivers", mapChunkData.rivers, riverShader);

		AddChild(terrain);
		AddChild(water);
		AddChild(waterShore);
		AddChild(rivers);
	}

	public override void _Ready() {
		// move the chunk into position
		var origin = HexUtils.HexToPixel(firstHex);
		Transform.Translated(new Vector3(origin.x, 0, origin.y));

		var watch = System.Diagnostics.Stopwatch.StartNew();
		GenerateMeshes();
		// GD.PrintS($"Chunk generate: {watch.ElapsedMilliseconds}ms");

		// handle toggling grid
		hexGrid.viewSettings.showGrid.Subscribe((bool showGrid) => {
			terrain.material.SetShaderParam("showGrid", showGrid);
		});

		// handle terrain mesh input
		terrain.MeshClickPos.Subscribe((Vector2 hexPosition) => {
			var hex = Hex.HexUtils.PixelToHexOffset(hexPosition);
			var cell = hexGrid.GetCell(hex);
			chunks.pressedCell.OnNext(cell);
		});

		terrain.MeshHoverPos.Subscribe((Vector2 hexPosition) => {
			var hex = Hex.HexUtils.PixelToHexOffset(hexPosition);
			var cell = hexGrid.GetCell(hex);
			chunks.hoveredCell.OnNext(cell);
		});
	}

	public void Reset() {
		terrain.Clear();
		water.Clear();
		waterShore.Clear();
		rivers.Clear();
	}

	public void GenerateMeshes() {
		terrain.GenerateMesh();
		water.GenerateMesh();
		waterShore.GenerateMesh();
		rivers.GenerateMesh();
	}

	public void Generate() {
		Reset();
		mapChunkData.Reset();
		mapChunkData.Calculate();
		GenerateMeshes();
	}
}
