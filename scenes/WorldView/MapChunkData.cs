using Godot;
using Hex;
using System.Threading;

public class MapChunkData {
    private readonly HexGrid grid;
    public readonly OffsetCoord coord;
    private readonly CountdownEvent doneEvent;

	private const float RIVER_BED_DEPTH = 1.5f;
	private const float RIVER_SHORE_DEPTH = RIVER_BED_DEPTH * 0.5f;
	private readonly Vector3 riverBedVector = new Vector3(0, RIVER_BED_DEPTH, 0);
	private readonly Vector3 riverShoreVector = new Vector3(0, RIVER_SHORE_DEPTH, 0);

	public ChunkMeshData terrain;
	public ChunkMeshData water;
	public ChunkMeshData waterShore;
	public ChunkMeshData rivers;

    public MapChunkData(
		HexGrid grid,
		OffsetCoord coord,
		CountdownEvent doneEvent
	) {
        this.grid = grid;
        this.coord = coord;
        this.doneEvent = doneEvent;

		terrain = new ChunkMeshData(true);
		water = new ChunkMeshData();
		waterShore = new ChunkMeshData(false, true);
		rivers = new ChunkMeshData();
    }

	public void ThreadPoolCallback(System.Object stateInfo) {
		Calculate();
		doneEvent.Signal();
	}

	public void Reset() {
		terrain.Clear();
		water.Clear();
		waterShore.Clear();
		rivers.Clear();
	}

	public void Calculate() {
		for (int col = 0; col < ChunksContainer.chunkSize.Col; col++) {
			for (int row = 0; row < ChunksContainer.chunkSize.Row; row++) {
				var hex = coord + new Hex.OffsetCoord(col, row);
				var cell = grid.GetCell(hex);
				for (int d = 0; d < 6; d++) {
					var dir = (Direction) d;
					Triangulate(cell, dir);
				}
			}
		}
		terrain.CreateSurface();
		water.CreateSurface();
		waterShore.CreateSurface();
		rivers.CreateSurface();
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

		// triangles from cell center to edge
		water.AddTriangle(center, e1.v1, e1.v2);
		water.AddTriangle(center, e1.v2, e1.v3);
		water.AddTriangle(center, e1.v3, e1.v4);
		water.AddTriangle(center, e1.v4, e1.v5);
		water.AddTriangle(center, e1.v5, e1.v6);
		water.AddTriangle(center, e1.v6, e1.v7);
		water.AddTriangle(center, e1.v7, e1.v8);
		water.AddTriangle(center, e1.v8, e1.v9);

		var v1_n = neighbor.WaterCenter + dir.Opposite().CornerRight().InnerPosition();
		var v2_n = neighbor.WaterCenter + dir.Opposite().CornerLeft().InnerPosition();
		var v1_c = v1.LinearInterpolate(v1_n, 0.5f);
		var v2_c = v2.LinearInterpolate(v2_n, 0.5f);
		EdgeVertices e2 = new EdgeVertices(v1_c, v2_c);

		waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
		waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
		waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
		waterShore.AddQuad(e1.v5, e1.v6, e2.v5, e2.v6);
		waterShore.AddQuad(e1.v6, e1.v7, e2.v6, e2.v7);
		waterShore.AddQuad(e1.v7, e1.v8, e2.v7, e2.v8);
		waterShore.AddQuad(e1.v8, e1.v9, e2.v8, e2.v9);

		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);

		var prev_neighbor = cell.GetNeighbor(dir.Prev());
		if (prev_neighbor != null) {
			var prev_opp_dir = dir.Prev().Opposite();
			var c = cell.WaterCenter + dir.CornerRight().InnerPosition();
			var p = prev_neighbor.WaterCenter + prev_opp_dir.CornerRight().InnerPosition();
			var n = neighbor.WaterCenter + dir.Opposite().CornerLeft().InnerPosition();
			var c_p = c.LinearInterpolate(p, 0.5f);
			var n_p = n.LinearInterpolate(p, 0.5f);
			var c_n = c.LinearInterpolate(n, 0.5f);

			if (prev_neighbor.IsUnderwater) {
				// cell is water, neighbor is land, prev_neighbor is water
				waterShore.AddQuad(c, p, c_n, n_p);
				waterShore.AddQuadUV(0f, 0f, 0f, 1f);
				// TODO: this can't be a quad, has to be 3 triangles
			} else {
				// cell is water, neighbor is land, prev_neighbor is land
				waterShore.AddTriangle(c_n, c_p, c);
				waterShore.AddTriangleUV(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f));
			}
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
		
		// hex centers
		TriangulateCenter(dir, cell, edge);

		// edges between hexes
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
		if (cell.IsUnderwater) {
			terrain.TriangulateEdgeFan(cell.Center, edge, cell.Color);
			return;
		}
		var E1 = cell.Center + dir.Points()[SidePoint.E1];
		var E2 = cell.Center + dir.Points()[SidePoint.E2];
		var E3 = cell.Center + dir.Points()[SidePoint.E3];
		var E4 = cell.Center + dir.Points()[SidePoint.E4];
		var E5 = cell.Center + dir.Points()[SidePoint.E5];
		var E6 = cell.Center + dir.Points()[SidePoint.E6];
		var E7 = cell.Center + dir.Points()[SidePoint.E7];
		var a1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.A];
		var a2 = cell.Center + dir.CornerRight().Points()[CornerPoint.A];
		var b1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.B];
		var b2 = cell.Center + dir.CornerRight().Points()[CornerPoint.B];
		var c1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.C];
		var c2 = cell.Center + dir.CornerRight().Points()[CornerPoint.C];
		var f1 = cell.Center + dir.CornerLeft().Points()[CornerPoint.F];
		var f2 = cell.Center + dir.CornerRight().Points()[CornerPoint.F];
		var C1 = cell.Center + dir.Points()[SidePoint.C1];
		var C2 = cell.Center + dir.Points()[SidePoint.C2];
		var C3 = cell.Center + dir.Points()[SidePoint.C3];
		var C4 = cell.Center + dir.Points()[SidePoint.C4];
		var S1 = cell.Center + dir.Points()[SidePoint.S1];
		var S2 = cell.Center + dir.Points()[SidePoint.S2];
		var S3 = cell.Center + dir.Points()[SidePoint.S3];
		var S4 = cell.Center + dir.Points()[SidePoint.S4];
		var S5 = cell.Center + dir.Points()[SidePoint.S5];
		var S6 = cell.Center + dir.Points()[SidePoint.S6];
		var L1 = cell.Center + dir.Points()[SidePoint.L1];
		var R1 = cell.Center + dir.Points()[SidePoint.R1];
		var b1_river = b1 - riverShoreVector;
		var b2_river = b2 - riverShoreVector;
		var c1_river = c1 - riverShoreVector;
		var c2_river = c2 - riverShoreVector;
		var f1_river = cell.Center + dir.CornerLeft().Points()[CornerPoint.F] - riverShoreVector;
		var f2_river = cell.Center + dir.CornerRight().Points()[CornerPoint.F] - riverShoreVector;
		var g1_river = cell.Center + dir.CornerLeft().Points()[CornerPoint.G] - riverShoreVector;
		var g2_river = cell.Center + dir.CornerRight().Points()[CornerPoint.G] - riverShoreVector;
		var center_river = cell.Center - riverShoreVector;
		var center_river_bank = cell.Center - riverBedVector;
		var C3_river = C3 - riverShoreVector;

		if (cell.HasRiver(dir)) {
			var E1_river_bank = E1 - riverBedVector;
			var E4_river = E4 - riverShoreVector;
			var E5_river = E5 - riverShoreVector;
			var E5_river_bank = E5 - riverBedVector;
			var C1_river_bank = C1 - riverBedVector;
			var C2_river_bank = C2 - riverBedVector;
			var C3_river_bank = C3 - riverBedVector;
			var S1_river = S1 - riverShoreVector;
			var S2_river = S2 - riverShoreVector;
			var S5_river = S5 - riverShoreVector;
			var S6_river = S6 - riverShoreVector;
			// INNER EDGE
			// river bank
			terrain.AddTriangle(edge.v1, E6, a1);
			terrain.AddTriangleColor(cell.Color);
			terrain.AddTriangle(a1, S3, b1);
			terrain.AddTriangleColor(cell.Color);
			terrain.AddQuad(a1, S3, E6, E2);
			terrain.AddQuadColor(cell.Color);

			terrain.AddTriangle(E7, edge.v9, a2);
			terrain.AddTriangleColor(cell.Color);
			terrain.AddTriangle(S4, a2, b2);
			terrain.AddTriangleColor(cell.Color);
			terrain.AddQuad(S4, a2, E3, E7);
			terrain.AddQuadColor(cell.Color);

			// river channel
			terrain.AddQuad(S3, S5_river, E2, E4_river);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(S5_river, C1_river_bank, E4_river, E1_river_bank);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(C1_river_bank, S6_river, E1_river_bank, E5_river);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(S6_river, S4, E5_river, E3);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(b1, S1_river, S3, S5_river);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(S1_river, C2_river_bank, S5_river, C1_river_bank);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(C2_river_bank, S2_river, C1_river_bank, S6_river);
			terrain.AddQuadColor(cell.Color);
			terrain.AddQuad(S2_river, b2, S6_river, S4);
			terrain.AddQuadColor(cell.Color);

			// river surface
			var E1_river = E1 - riverShoreVector;
			var C2_river = C2 - riverShoreVector;
			rivers.AddQuad(E4_river, S1_river, E1_river, C2_river);
			rivers.AddQuad(E1_river, C2_river, E5_river, S2_river);
			
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
				terrain.TriangulateCenterOuter(cell, edge, dir, cell.Color);

				terrain.AddTriangle(b1, S1, L1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S1, C2, L1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, R1, L1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, S2, R1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2, b2, R1);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				terrain.AddTriangle(b1, L1, c1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(R1, b2, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(L1, C3_river, c1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(L1, R1, C3_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(R1, c2_river, C3_river);
				terrain.AddTriangleColor(cell.Color);

				terrain.AddTriangle(c1_river, C3_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C3_river, c2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(c1_river, C3_river, center_river);
				rivers.AddTriangle(C3_river, c2_river, center_river);
			} else if (cell.HasRiver(dir.Prev())) {
				// TYPE 3: straight connector from right
				// river bank
				terrain.TriangulateCenterOuter(cell, edge, dir, cell.Color);

				terrain.AddTriangle(b1, S1, c1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S1, C2, c1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, R1, c1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C2, S2, R1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2, b2, R1);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				terrain.AddTriangle(c1, R1, g1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(R1, c2_river, g1_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(R1, b2, c2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(g1_river, c2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				// river surface
				rivers.AddTriangle(g1_river, c2_river, center_river);
			} else if (cell.HasRiver(dir.Next())) {
				// TYPE 4: straight connector from left
				// river bank
				terrain.TriangulateCenterOuter(cell, edge, dir, cell.Color);

				terrain.AddTriangle(S2, b2, c2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C1, S2, c2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(L1, C1, c2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S1, C1, L1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(b1, S1, L1);
				terrain.AddTriangleColor(cell.Color);

				// river channel
				terrain.AddTriangle(L1, c2, g2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(c1_river, L1, g2_river);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(b1, L1, c1_river);
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
				var C4_river = C4 - riverShoreVector;

				terrain.TriangulateCenterOuter(cell, edge, dir, cell.Color);

				terrain.AddTriangle(b1, S1, c1);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(S2, b2, c2);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddQuad(c1, C3, S1, C2);
				terrain.AddQuadColor(cell.Color);
				terrain.AddQuad(C3, c2, C2, S2);
				terrain.AddQuadColor(cell.Color);

				terrain.AddQuad(g1_river, C4_river, c1, C3);
				terrain.AddQuadColor(cell.Color);
				terrain.AddQuad(C4_river, g2_river, C3, c2);
				terrain.AddQuadColor(cell.Color);

				terrain.AddTriangle(g1_river, C4_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);
				terrain.AddTriangle(C4_river, g2_river, center_river_bank);
				terrain.AddTriangleColor(cell.Color);

				rivers.AddTriangle(g1_river, g2_river, center_river);
			} else {
				terrain.TriangulateEdgeFan(cell.Center, edge, cell.Color);
			}
		}
	}

	private void TriangulateConnection(Direction dir, HexCell cell, EdgeVertices e1) {
		var neighbor = cell.GetNeighbor(dir);
		if (neighbor == null) {
			return;
		}
		var neighbor_dir = dir.Opposite();
		var v1_n = neighbor.Center + neighbor_dir.CornerRight().InnerPosition();
		var v2_n = neighbor.Center + neighbor_dir.CornerLeft().InnerPosition();
		var v1_c = e1.v1.LinearInterpolate(v1_n, 0.5f);
		var v2_c = e1.v9.LinearInterpolate(v2_n, 0.5f);
		var e2 = new EdgeVertices(v1_c, v2_c);
		var e3 = new EdgeVertices(v1_n, v2_n);
		var blended_color = (cell.Color + neighbor.Color) / 2f;

		terrain.TriangulateEdgeStrip(e1, cell.Color, e2, blended_color);
		terrain.TriangulateEdgeStrip(e2, blended_color, e3, neighbor.Color);

		TriangulateCornerConnection(dir, cell, neighbor);
	}

	private void TriangulateCornerConnection(Direction dir, HexCell cell, HexCell neighbor) {
		var prev_neighbor = cell.GetNeighbor(dir.Prev());
		if (dir > 0 && prev_neighbor != null) {
			var prev_opp_dir = dir.Prev().Opposite();
			var c = cell.Center + dir.CornerRight().InnerPosition();
			var p = prev_neighbor.Center + prev_opp_dir.CornerRight().InnerPosition();
			var n = neighbor.Center + dir.Opposite().CornerLeft().InnerPosition();
			var c_p = c.LinearInterpolate(p, 0.5f);
			var n_p = n.LinearInterpolate(p, 0.5f);
			var c_n = c.LinearInterpolate(n, 0.5f);
			var color_c_n = (neighbor.Color + cell.Color) / 2f; 
			var color_n_p = (neighbor.Color + prev_neighbor.Color) / 2f; 
			var color_c_p = (cell.Color + prev_neighbor.Color) / 2f;

			terrain.AddTriangle(n, n_p, c_n);
			terrain.AddTriangleColor(neighbor.Color, color_n_p, color_c_n);
			terrain.AddTriangle(c_n, c_p, c);
			terrain.AddTriangleColor(color_c_n, color_c_p, cell.Color);
			terrain.AddTriangle(c_n, n_p, c_p);
			terrain.AddTriangleColor(color_c_n, color_n_p, color_c_p);
			terrain.AddTriangle(n_p, p, c_p);
			terrain.AddTriangleColor(color_n_p, prev_neighbor.Color, color_c_p);
		}
	}

	private void TriangulateRiverConnection(Direction dir, HexCell cell, EdgeVertices e1) {
		var neighbor = cell.GetNeighbor(dir);
		if (neighbor == null) {
			return;
		}

		var neighbor_dir = dir.Opposite();
		var v1_n = neighbor.Center + neighbor_dir.CornerRight().InnerPosition();
		var v2_n = neighbor.Center + neighbor_dir.CornerLeft().InnerPosition();
		var v1_c = e1.v1.LinearInterpolate(v1_n, 0.5f);
		var v2_c = e1.v9.LinearInterpolate(v2_n, 0.5f);
		var e2 = new EdgeVertices(v1_c, v2_c);
		var e3 = new EdgeVertices(v1_n, v2_n);

		var blended_color = (cell.Color + neighbor.Color) / 2f;

		terrain.TriangulateEdgeStripRiver(e1, cell.Color, e2, blended_color, riverBedVector, riverShoreVector);
		terrain.TriangulateEdgeStripRiver(e2, blended_color, e3, neighbor.Color, riverBedVector, riverShoreVector);

		if (!neighbor.IsUnderwater && cell.IsUnderwater) {
			// TODO: river mouth
		} else {
			rivers.AddQuad(e1.v4 - riverShoreVector, e1.v5 - riverShoreVector, e2.v4 - riverShoreVector, e2.v5 - riverShoreVector);
			rivers.AddQuad(e1.v5 - riverShoreVector, e1.v6 - riverShoreVector, e2.v5 - riverShoreVector, e2.v6 - riverShoreVector);
			rivers.AddQuad(e2.v4 - riverShoreVector, e2.v5 - riverShoreVector, e3.v4 - riverShoreVector, e3.v5 - riverShoreVector);
			rivers.AddQuad(e2.v5 - riverShoreVector, e2.v6 - riverShoreVector, e3.v5 - riverShoreVector, e3.v6 - riverShoreVector);
		}

		TriangulateCornerConnection(dir, cell, neighbor);
	}

	Vector3 WithHeight(Vector2 vector, double height) {
		return new Vector3(vector.x, (float) height, vector.y);
	}
}
