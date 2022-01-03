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
    private HexMesh rivers;
    private const float RIVER_BANK_DEPTH = 2.5f;
	private const float RIVER_DEPTH = RIVER_BANK_DEPTH / 2f;

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
		this.rivers = new HexMesh();
		Generate();
		var origin = HexUtils.HexToPixel(firstHex);
		Transform.Translated(new Vector3(origin.x, 0, origin.y));
		// GD.PrintS($"Chunk generate: {watch.ElapsedMilliseconds}ms");

		hexGrid.viewSettings.showGrid.Subscribe((bool showGrid) => {
			var material = (ShaderMaterial) terrain.mesh.SurfaceGetMaterial(0);
			material.SetShaderParam("showGrid", showGrid);
		});
	}

	public void Generate() {
		foreach (Node n in GetChildren()) {
			RemoveChild(n);
		}
		terrain.Clear();
		water.Clear();
		rivers.Clear();
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

		// Terrain mesh
		terrain.GenerateMesh();
		terrain.mesh.SurfaceSetMaterial(0, (ShaderMaterial) ResourceLoader.Load("res://scenes/WorldView/materials/GridShader.tres"));

		var terrainMeshInstance = new MeshInstance();
		terrainMeshInstance.Name = "Terrain";
		terrainMeshInstance.Mesh = terrain.mesh;
		AddChild(terrainMeshInstance);

		// terrain collision
		var staticBody = new StaticBody();
		var collision = new CollisionShape();
		collision.Name = "TerrainCollision";
		collision.Shape = terrain.mesh.CreateTrimeshShape();
		staticBody.AddChild(collision);
		staticBody.Connect("input_event", this, nameof(_handle_input));
		AddChild(staticBody);

		// water mesh
		water.GenerateMesh();
		water.mesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/materials/Water.tres"));

		var waterMeshInstance = new MeshInstance();
		waterMeshInstance.Name = "Water";
		waterMeshInstance.Mesh = water.mesh;
		AddChild(waterMeshInstance);

		// Rivers mesh
		rivers.GenerateMesh();
		rivers.mesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/materials/Water.tres"));

		var riversMeshInstance = new MeshInstance();
		riversMeshInstance.Name = "Rivers";
		riversMeshInstance.Mesh = rivers.mesh;
		AddChild(riversMeshInstance);
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
		
		TriangulateCenter(dir, cell, edge);

		if (d <= 2) {
			if (cell.HasRiver(dir)) {
				TriangulateRiverConnection(dir, cell, edge);
			} else {
				TriangulateConnection(dir, cell, edge);
			}
		}

		if (cell.IsUnderwater) {
			TriangulateWater(cell, dir);
		}
	}

	private void TriangulateCenter(Direction dir, HexCell cell, EdgeVertices edge) {
		var b1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.B];
		var b2 = cell.Center + dir.CornerRight().Points()[CornerPoint.B];
		var c1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.C];
		var c2 = cell.Center + dir.CornerRight().Points()[CornerPoint.C];
		var C2 = cell.Center + dir.Points()[SidePoint.C2];
		var L1 = cell.Center + dir.Points()[SidePoint.L1];
		var R1 = cell.Center + dir.Points()[SidePoint.R1];
		var b1_river = b1 - new Vector3(0, RIVER_DEPTH, 0);
		var b2_river = b2 - new Vector3(0, RIVER_DEPTH, 0);
		var c1_river = c1 - new Vector3(0, RIVER_DEPTH, 0);
		var c2_river = c2 - new Vector3(0, RIVER_DEPTH, 0);
		var center_river = cell.Center - new Vector3(0, RIVER_DEPTH, 0);
		var center_river_bank = cell.Center - new Vector3(0, RIVER_BANK_DEPTH, 0);

		if (cell.HasRiver(dir)) {
			// TYPE 1: straight river segment
			var e1 = edge.v3;
			e1.y -= RIVER_BANK_DEPTH;
			C2.y -= RIVER_BANK_DEPTH;
			var center = cell.Center;
			center.y -= RIVER_BANK_DEPTH;
			// river bank
			terrain.AddTriangle(edge.v1, edge.v2, b1);
			terrain.AddTriangleColor(cell.Color);

			terrain.AddTriangle(edge.v4, edge.v5, b2);
			terrain.AddTriangleColor(cell.Color);

			// river channel
			terrain.AddQuad(edge.v2, b1, e1, C2);
			terrain.AddQuadColor(cell.Color);

			terrain.AddQuad(e1, C2, edge.v4, b2);
			terrain.AddQuadColor(cell.Color);

			terrain.AddTriangle(b1, C2, center);
			terrain.AddTriangleColor(cell.Color);

			terrain.AddTriangle(C2, b2, center);
			terrain.AddTriangleColor(cell.Color);

			// river surface
			var e2_river = edge.v2 - new Vector3(0, RIVER_DEPTH, 0);
			var e3_river = edge.v4 - new Vector3(0, RIVER_DEPTH, 0);
			rivers.AddQuad(e2_river, b1_river, e3_river, b2_river);
			rivers.AddTriangle(b1_river, b2_river, center_river);
		} else {
			if (cell.HasRiver(dir.Next()) && cell.HasRiver(dir.Prev())) {
				// TYPE 2: River on both sides

				// river bank
				terrain.TriangulateInnerEdgeFan(edge, b1, b2, cell.Color);

				terrain.AddTriangle(b1, C2, L1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, R1, L1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, b2, R1);
				terrain.AddTriangleColor(cell.Color);


				// river channel
				
				terrain.AddTriangle(b1, L1, c1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(R1, b2, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddQuad(c1_river, c2_river, L1, R1);
				terrain.AddQuadColor(cell.Color);

				terrain.AddTriangle(c1_river, c2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(b1_river, b2_river, center_river);
			} else if (cell.HasRiver(dir.Prev()) && !cell.HasRiver(dir.Next()) && cell.HasRiver(dir.Next().Next())) {
				// TYPE 3: straight connector from right
				// river bank
				terrain.TriangulateInnerEdgeFan(edge, b1, b2, cell.Color);
				terrain.AddTriangle(b1, C2, c1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, b2, c1);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				var f1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.F] - new Vector3(0, RIVER_DEPTH, 0);
				terrain.AddTriangle(R1, b2, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1, R1, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1, c2_river, f1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(f1, c2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(c1_river, b2_river, center_river);
			} else if (cell.HasRiver(dir.Next()) && !cell.HasRiver(dir.Prev()) && cell.HasRiver(dir.Prev().Prev())) {
				// TYPE 4: straight connector from left
				// river bank
				terrain.TriangulateInnerEdgeFan(edge, b1, b2, cell.Color);
				terrain.AddTriangle(C2, b2, c2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(b1, C2, c2);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				var f2 = cell.Center + dir.CornerRight().Points()[CornerPoint.F] - new Vector3(0, RIVER_DEPTH, 0);
				terrain.AddTriangle(b1, L1, c1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(L1, c2, c1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, c2, f2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, f2, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(b1_river, c2_river, center_river);
			} else {
				terrain.TriangulateEdgeFan(cell.Center, edge, cell.Color);
			}
		}
	}

	private void TriangulateConnection(Direction dir, HexCell cell, EdgeVertices edge) {
		var neighbor = cell.GetNeighbor(dir);
		if (neighbor == null) {
			return;
		}
		var neighbor_dir = dir.Opposite();
		var v2_n = neighbor.Center + neighbor_dir.CornerRight().InnerPosition();
		var v1_n = neighbor.Center + neighbor_dir.CornerLeft().InnerPosition();
		var e2 = new EdgeVertices(v2_n, v1_n);

		terrain.TriangulateEdgeStrip(edge, cell.Color, e2, neighbor.Color);
		TriangulateCornerConnection(dir, cell, neighbor, edge, e2);
	}

	private void TriangulateRiverConnection(Direction dir, HexCell cell, EdgeVertices edge) {
		var neighbor = cell.GetNeighbor(dir);
		if (neighbor == null) {
			return;
		}
		var neighbor_dir = dir.Opposite();
		var v2_n = neighbor.Center + neighbor_dir.CornerRight().InnerPosition();
		var v1_n = neighbor.Center + neighbor_dir.CornerLeft().InnerPosition();
		edge.v3.y -= RIVER_BANK_DEPTH;
		var e2 = new EdgeVertices(v2_n, v1_n);
		e2.v3.y -= RIVER_BANK_DEPTH;

		terrain.TriangulateEdgeStrip(edge, cell.Color, e2, neighbor.Color);
		TriangulateCornerConnection(dir, cell, neighbor, edge, e2);

		rivers.AddQuad(
			edge.v2 - new Vector3(0, RIVER_DEPTH, 0),
			edge.v4 - new Vector3(0, RIVER_DEPTH, 0),
			e2.v2 - new Vector3(0, RIVER_DEPTH, 0),
			e2.v4 - new Vector3(0, RIVER_DEPTH, 0)
		);
	}

	private void TriangulateCornerConnection(Direction dir, HexCell cell, HexCell neighbor, EdgeVertices e1, EdgeVertices e2) {
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
