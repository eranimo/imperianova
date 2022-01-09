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
	private const float RIVER_DEPTH = RIVER_BANK_DEPTH * 0.5f;
	private const float RIVER_DEPTH_75 = RIVER_BANK_DEPTH * 0.25f;
	private const float RIVER_DEPTH_25 = RIVER_BANK_DEPTH * 0.75f;

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
		var f1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.F];
		var f2 = cell.Center + dir.CornerRight().Points()[CornerPoint.F];
		var C2 = cell.Center + dir.Points()[SidePoint.C2];
		var C3 = cell.Center + dir.Points()[SidePoint.C3];
		var C4 = cell.Center + dir.Points()[SidePoint.C4];
		var S1 = cell.Center + dir.Points()[SidePoint.S1];
		var S2 = cell.Center + dir.Points()[SidePoint.S2];
		var L1 = cell.Center + dir.Points()[SidePoint.L1];
		var R1 = cell.Center + dir.Points()[SidePoint.R1];
		var b1_river = b1 - new Vector3(0, RIVER_DEPTH, 0);
		var b2_river = b2 - new Vector3(0, RIVER_DEPTH, 0);
		var c1_river = c1 - new Vector3(0, RIVER_DEPTH, 0);
		var c2_river = c2 - new Vector3(0, RIVER_DEPTH, 0);
		var f1_river = cell.Center + dir.CornerLeft().Points()[CornerPoint.F] - new Vector3(0, RIVER_DEPTH, 0);
		var f2_river = cell.Center + dir.CornerRight().Points()[CornerPoint.F] - new Vector3(0, RIVER_DEPTH, 0);
		var g1_river = cell.Center + dir.CornerLeft().Points()[CornerPoint.G] - new Vector3(0, RIVER_DEPTH, 0);
		var g2_river = cell.Center + dir.CornerRight().Points()[CornerPoint.G] - new Vector3(0, RIVER_DEPTH, 0);
		var center_river = cell.Center - new Vector3(0, RIVER_DEPTH, 0);
		var center_river_bank = cell.Center - new Vector3(0, RIVER_BANK_DEPTH, 0);

		if (cell.HasRiver(dir)) {
			var e1_river_bank = edge.v3 - new Vector3(0, RIVER_BANK_DEPTH, 0);
			var e4_river = cell.Center + dir.Points()[SidePoint.E4] - new Vector3(0, RIVER_DEPTH, 0);
			var e5_river = cell.Center + dir.Points()[SidePoint.E5] - new Vector3(0, RIVER_DEPTH, 0);
			var C2_river_bank = C2 - new Vector3(0, RIVER_BANK_DEPTH, 0);
			var C3_river_bank = C3 - new Vector3(0, RIVER_BANK_DEPTH, 0);
			var S1_river = S1 - new Vector3(0, RIVER_DEPTH, 0);
			var S2_river = S2 - new Vector3(0, RIVER_DEPTH, 0);
			// INNER EDGE
			// river bank
			terrain.AddTriangle(edge.v1, edge.v2, b1);
			terrain.AddTriangleColor(cell.Color);
			terrain.AddTriangle(edge.v4, edge.v5, b2);
			terrain.AddTriangleColor(cell.Color);

			// river channel
			terrain.AddQuad(edge.v2, b1, e4_river, S1_river); // 1
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(e4_river, S1_river, e1_river_bank, C2_river_bank); // 2
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(e1_river_bank, C2_river_bank, e5_river, S2_river); // 3
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(e5_river, S2_river, edge.v4, b2);
			terrain.AddQuadColor(cell.Color);

			// river surface
			var e2_river = edge.v2 - new Vector3(0, RIVER_DEPTH, 0);
			var e3_river = edge.v4 - new Vector3(0, RIVER_DEPTH, 0);
			var e1_river = edge.v3 - new Vector3(0, RIVER_DEPTH, 0);
			var C2_river = C2 - new Vector3(0, RIVER_DEPTH, 0);
			var C3_river = C3 - new Vector3(0, RIVER_DEPTH, 0);
			rivers.AddQuad(e4_river, S1_river, e1_river, C2_river);
			rivers.AddQuad(e1_river, C2_river, e5_river, S2_river);
			
			if (cell.HasRiverFlowEither(dir, dir.Prev()) && cell.HasRiverFlowEither(dir, dir.Next())) {
				// TYPE 8: river on both
				terrain.AddTriangle(b1, S1_river, f1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S1_river, C2_river_bank, f1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(f1_river, C2_river_bank, C3_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(f1_river, C3_river_bank, center_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, S2_river, f2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2_river, b2, f2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, f2_river, C3_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C3_river_bank, f2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				rivers.AddTriangle(S1_river, C2_river, f1_river);
				rivers.AddTriangle(f1_river, C2_river, C3_river);
				rivers.AddTriangle(f1_river, C3_river, center_river);
				rivers.AddTriangle(C2_river, S2_river, f2_river);
				rivers.AddTriangle(C2_river, f2_river, C3_river);
				rivers.AddTriangle(C3_river, f2_river, center_river);
			} else if (cell.HasRiverFlowEither(dir, dir.Prev())) {
				var c2_river_75 = c2 - new Vector3(0, RIVER_DEPTH_75, 0);
				// TYPE 6: river on right, same flow direction
				terrain.AddTriangle(b1, S1_river, c1_river); // 1
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, c1_river, S1_river); // 2
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, C3_river_bank, c1_river); // 3
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, S2_river, f2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2_river, b2, f2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, f2_river, C3_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C3_river_bank, f2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, C3_river_bank, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				rivers.AddTriangle(S1_river, C2_river, c1_river);
				rivers.AddTriangle(C2_river, C3_river, c1_river);
				rivers.AddTriangle(c1_river, C3_river, center_river);
				rivers.AddTriangle(C2_river, S2_river, f2_river);
				rivers.AddTriangle(C2_river, f2_river, C3_river);
				rivers.AddTriangle(C3_river, f2_river, center_river);
			} else if (cell.HasRiverFlowEither(dir, dir.Next())) {
				// TYPE 7: river on left, same flow direction
				terrain.AddTriangle(b1, S1_river, f1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S1_river, C2_river_bank, f1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(f1_river, C2_river_bank, C3_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(f1_river, C3_river_bank, center_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, c2_river, C3_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, S2_river, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2_river, b2, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C3_river_bank, c2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				rivers.AddTriangle(S1_river, C2_river, f1_river);
				rivers.AddTriangle(f1_river, C2_river, C3_river);
				rivers.AddTriangle(f1_river, C3_river, center_river);
				rivers.AddTriangle(C2_river, c2_river, C3_river);
				rivers.AddTriangle(C2_river, S2_river, c2_river);
				rivers.AddTriangle(C3_river, c2_river, center_river);
			} else {
				// TYPE 1: straight river segment
				terrain.AddTriangle(b1, S1_river, c1_river); // 1
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, c1_river, S1_river); // 2
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, C3_river_bank, c1_river); // 3
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, c2_river, C3_river_bank); // 4
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2_river_bank, S2_river, c2_river); // 5
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2_river, b2, c2_river); // 6
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, C3_river_bank, center_river_bank); // 7
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C3_river_bank, c2_river, center_river_bank); // 8
				terrain.AddTriangleColor(cell.Color);

				rivers.AddQuad(S1_river, c1_river, C2_river, center_river);
				rivers.AddQuad(C2_river, center_river, S2_river, c2_river);
			}
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
				rivers.AddTriangle(c1_river, c2_river, center_river);
			} else if (cell.HasRiver(dir.Prev())) {
				// TYPE 3: straight connector from right
				// river bank
				terrain.TriangulateInnerEdgeFan(edge, b1, b2, cell.Color);
				terrain.AddTriangle(b1, C2, c1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, b2, c1);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				terrain.AddTriangle(c1, b2, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1, c2_river, g1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(g1_river, c2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(g1_river, c2_river, center_river);
			} else if (cell.HasRiver(dir.Next())) {
				// TYPE 4: straight connector from left
				// river bank
				terrain.TriangulateInnerEdgeFan(edge, b1, b2, cell.Color);
				terrain.AddTriangle(C2, b2, c2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(b1, C2, c2);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				terrain.AddTriangle(b1, c2, c1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, c2, g2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, g2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(c1_river, g2_river, center_river);
			} else if (
				// handle wide turns
				(cell.HasRiver(dir.Prev().Prev()) && 
				cell.HasRiver(dir.Next().Next()))
				||
				// handle narrow turns and end cap
				(cell.HasRiver(dir.Prev().Prev().Prev()) && 
				cell.HasRiver(dir.Next().Next().Next()))
				||
				// handle sides of end cap
				(
					(cell.HasRiver(dir.Prev().Opposite()) && cell.HasRiver(dir.Next().Next())) ||
					(cell.HasRiver(dir.Next().Opposite()) && cell.HasRiver(dir.Prev().Prev()))
				)
			) {
				// TYPE 5: double connector (left and right)
				var C4_river = C4 - new Vector3(0, RIVER_DEPTH, 0);

				terrain.TriangulateInnerEdgeFan(edge, b1, b2, cell.Color);
				terrain.AddQuad(b1, c1, C2, C3);
				terrain.AddQuadColor(cell.Color);
				terrain.AddQuad(C2, C3, b2, c2);
				terrain.AddQuadColor(cell.Color);

				terrain.AddQuad(c1, g1_river, C3, C4_river);
				terrain.AddQuadColor(cell.Color);
				terrain.AddQuad(C3, C4_river, c2, g2_river);
				terrain.AddQuadColor(cell.Color);

				terrain.AddTriangle(g1_river, g2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				rivers.AddTriangle(g1_river, g2_river, center_river);
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

	private void TriangulateRiverConnection(Direction dir, HexCell cell, EdgeVertices e1) {
		var neighbor = cell.GetNeighbor(dir);
		if (neighbor == null) {
			return;
		}
		var neighbor_dir = dir.Opposite();
		var v2_n = neighbor.Center + neighbor_dir.CornerRight().InnerPosition();
		var v1_n = neighbor.Center + neighbor_dir.CornerLeft().InnerPosition();
		var e1_v3_bank = e1.v3 - new Vector3(0, RIVER_BANK_DEPTH, 0);
		var e1_v3_river = e1.v3 - new Vector3(0, RIVER_DEPTH, 0);
		var e2 = new EdgeVertices(v2_n, v1_n);
		var e2_v3_bank = e2.v3 - new Vector3(0, RIVER_BANK_DEPTH, 0);
		var e2_v3_river = e2.v3 - new Vector3(0, RIVER_DEPTH, 0);

		var e1_river_a = e1.v2.LinearInterpolate(e1.v3, 0.5f);
		e1_river_a.y -= RIVER_DEPTH;
		var e1_river_b = e1.v3.LinearInterpolate(e1.v4, 0.5f);
		e1_river_b.y -= RIVER_DEPTH;
		var e2_river_a = e2.v2.LinearInterpolate(e2.v3, 0.5f);
		e2_river_a.y -= RIVER_DEPTH;
		var e2_river_b = e2.v3.LinearInterpolate(e2.v4, 0.5f);
		e2_river_b.y -= RIVER_DEPTH;

		terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2); // 1
		terrain.AddQuadColor(cell.Color, neighbor.Color);
		terrain.AddQuad(e1.v2, e1_river_a, e2.v2, e2_river_a); // 2
		terrain.AddQuadColor(cell.Color, neighbor.Color);
		terrain.AddQuad(e1_river_a, e1_v3_bank, e2_river_a, e2_v3_bank); // 3
		terrain.AddQuadColor(cell.Color, neighbor.Color);
		terrain.AddQuad(e1_v3_bank, e1_river_b, e2_v3_bank, e2_river_b); // 4
		terrain.AddQuadColor(cell.Color, neighbor.Color);
		terrain.AddQuad(e1_river_b, e1.v4, e2_river_b, e2.v4); // 5
		terrain.AddQuadColor(cell.Color, neighbor.Color);
		terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5); // 6
		terrain.AddQuadColor(cell.Color, neighbor.Color);

		TriangulateCornerConnection(dir, cell, neighbor, e1, e2);

		rivers.AddQuad(e1_river_a, e1_v3_river, e2_river_a, e2_v3_river);
		rivers.AddQuad(e1_v3_river, e1_river_b, e2_v3_river, e2_river_b);
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
