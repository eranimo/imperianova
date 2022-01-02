using Godot;
using Godot.Collections;
using System;
using Hex;
using LibNoise.Primitive;


public struct EdgeVertices {
	public Vector3 v1, v2, v3, v4, v5;

	public EdgeVertices(Vector3 corner1, Vector3 corner2) {
		v1 = corner1;
		v2 = corner1.LinearInterpolate(corner2, 0.25f);
		v3 = corner1.LinearInterpolate(corner2, 0.5f);
		v4 = corner1.LinearInterpolate(corner2, 0.75f);
		v5 = corner2;
	}
}

public class MapChunk : StaticBody {
    private readonly ChunksContainer chunks;
    private readonly HexGrid hexGrid;
    private readonly OffsetCoord firstHex;
    private readonly OffsetCoord chunkSize;
    private Random rng;

    private SimplexPerlin noise;
    private HexMesh terrain;
    private HexMesh water;

    public MapChunk(
		ChunksContainer chunks,
		HexGrid hexGrid,
		OffsetCoord firstHex,
		OffsetCoord chunkSize
	) {
        this.chunks = chunks;
        this.hexGrid = hexGrid;
        this.firstHex = firstHex;
        this.chunkSize = chunkSize;
    }

	public override void _Ready() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		rng = new Random();
		this.terrain = new HexMesh();
		this.water = new HexMesh();
		Generate();
		var origin = HexUtils.HexToPixel(firstHex);
		Transform.Translated(new Vector3(origin.x, 0, origin.y));
		// GD.PrintS($"Chunk generate: {watch.ElapsedMilliseconds}ms");
	}

	public void Generate() {
		foreach (Node n in GetChildren()) {
			RemoveChild(n);
		}
		terrain.Clear();
		water.Clear();
		generateMesh();
	}

	private void generateMesh() {
		for (int col = 0; col < chunkSize.Col; col++) {
			for (int row = 0; row < chunkSize.Row; row++) {
				var hex = firstHex + new Hex.OffsetCoord(col, row);
				var cell = hexGrid.GetCell(hex);
				for (int d = 0; d < 6; d++) {
					var dir = (Direction) d;
					Triangulate(cell, dir);
				}
			}
		}
		var terrainMesh = terrain.GenerateMesh();
		terrainMesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/materials/GridShader.tres"));

		var terrainMeshInstance = new MeshInstance();
		terrainMeshInstance.Name = "Terrain";
		terrainMeshInstance.Mesh = terrainMesh;
		AddChild(terrainMeshInstance);

		var staticBody = new StaticBody();
		var collision = new CollisionShape();
		collision.Name = "TerrainCollision";
		collision.Shape = terrainMesh.CreateTrimeshShape();
		staticBody.AddChild(collision);
		staticBody.Connect("input_event", this, nameof(_handle_input));
		AddChild(staticBody);

		var waterMesh = water.GenerateMesh();
		waterMesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/materials/Water.tres"));

		var waterMeshInstance = new MeshInstance();
		waterMeshInstance.Name = "Water";
		waterMeshInstance.Mesh = waterMesh;
		AddChild(waterMeshInstance);
	}

	private void _handle_input(Camera camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx) {
		var o = this.GlobalTransform.origin;
		var hexPosition = new Vector2(o.x, o.z) + new Vector2(position.x, position.z);
		var hex = Hex.HexUtils.PixelToHexOffset(hexPosition - Hex.HexUtils.HexCenter);
		var cell = hexGrid.GetCell(hex);

		if (@event.IsActionPressed("ui_select")) {
			// GD.PrintS("MapChunk click", cell.Position);
			chunks.pressedCell.OnNext(cell);
		} else {
			// GD.PrintS("MapChunk hover", cell.Position);
			chunks.hoveredCell.OnNext(cell);
		}
	}

	private void TriangulateWater(HexCell cell, Direction dir) {
		var neighbor = cell.GetNeighbor(dir);
		var center = WithHeight(HexUtils.HexToPixelCenter(cell.Position), cell.WaterLevel);
		if (neighbor != null && !neighbor.IsUnderwater) {
			TriangulateWaterShore(dir, cell, neighbor, center);
		} else {
			TriangulateOpenWater(dir, cell, neighbor, center);
		}
	}

	private void TriangulateWaterShore(Direction dir, HexCell cell, HexCell neighbor, Vector3 center) {
		var v1 = center + dir.CornerLeft().InnerPosition();
		var v2 = center + dir.CornerRight().InnerPosition();
		EdgeVertices e1 = new EdgeVertices(v1, v2);
		water.AddTriangle(center, e1.v1, e1.v2);
		water.AddTriangle(center, e1.v2, e1.v3);
		water.AddTriangle(center, e1.v3, e1.v4);
		water.AddTriangle(center, e1.v4, e1.v5);

		var v2_n = neighbor.WaterCenter + dir.Opposite().CornerRight().InnerPosition();
		var v1_n = neighbor.WaterCenter + dir.Opposite().CornerLeft().InnerPosition();
		EdgeVertices e2 = new EdgeVertices(v2_n, v1_n);
		water.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		water.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
		water.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
		water.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

		var prev_neighbor = cell.GetNeighbor(dir.Prev());
		if (prev_neighbor != null) {
			var prev_opp_dir = dir.Prev().Opposite();
			var prev_opp_center = prev_neighbor.Center;
			prev_opp_center.y = (float) prev_neighbor.WaterLevel;
			var v2_prev_neighbor = prev_opp_center + prev_opp_dir.CornerRight().InnerPosition();

			water.AddTriangle(e1.v5, e2.v5, v2_prev_neighbor);
		}
	}

	private void TriangulateOpenWater(Direction dir, HexCell cell, HexCell neighbor, Vector3 center) {
		// add center triangle
		var v1 = center + dir.CornerLeft().InnerPosition();
		var v2 = center + dir.CornerRight().InnerPosition();
		water.AddTriangle(center, v1, v2);

		var d = (int) dir;
		if (d <= 2) {
			if (neighbor == null || !neighbor.IsUnderwater) {
				return;
			}

			// add edge strip
			var neighbor_dir = dir.Opposite();
			var v2_n = neighbor.WaterCenter + neighbor_dir.CornerRight().InnerPosition();
			var v1_n = neighbor.WaterCenter + neighbor_dir.CornerLeft().InnerPosition();
			var neighbor_color = neighbor.Color;

			water.AddQuad(v1, v2, v2_n, v1_n);

			// add corner
			var prev_neighbor = cell.GetNeighbor(dir.Prev());
			if (dir > 0 && prev_neighbor != null && prev_neighbor.IsUnderwater) {
				var prev_opp_dir = dir.Prev().Opposite();
				var prev_opp_center = prev_neighbor.WaterCenter;
				var v2_prev_neighbor = prev_opp_center + prev_opp_dir.CornerRight().InnerPosition();

				water.AddTriangle(v2, v1_n, v2_prev_neighbor);
			}
		}
	}

	private void Triangulate(HexCell cell, Direction dir) {
		var d = (int) dir;
		var v1 = cell.Center + dir.CornerLeft().InnerPosition();
		var v2 = cell.Center + dir.CornerRight().InnerPosition();
		var edge = new EdgeVertices(v1, v2);
		
		terrain.TriangulateEdgeFan(cell.Center, edge, cell.Color);

		if (d <= 2) {
			TriangulateConnection(dir, cell, edge);
		}

		if (cell.IsUnderwater) {
			TriangulateWater(cell, dir);
		}
	}

	private void TriangulateConnection(Direction dir, HexCell cell, EdgeVertices e1) {
		var neighbor = cell.GetNeighbor(dir);
		if (neighbor == null) {
			return;
		}
		var neighbor_dir = dir.Opposite();
		var v2_n = neighbor.Center + neighbor_dir.CornerRight().InnerPosition();
		var v1_n = neighbor.Center + neighbor_dir.CornerLeft().InnerPosition();
		var e2 = new EdgeVertices(v2_n, v1_n);

		terrain.TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);

		// corner
		var prev_neighbor = cell.GetNeighbor(dir.Prev());
		if (dir > 0 && prev_neighbor != null) {
			var prev_opp_dir = dir.Prev().Opposite();
			var v2_prev_neighbor = prev_neighbor.Center + prev_opp_dir.CornerRight().InnerPosition();

			terrain.AddTriangle(e1.v5, e2.v5, v2_prev_neighbor);
			terrain.AddTriangleColor(cell.Color, neighbor.Color, prev_neighbor.Color);
		}
	}

	Vector3 WithHeight(Vector2 vector, double height) {
		return new Vector3(vector.x, (float) height, vector.y);
	}
}
