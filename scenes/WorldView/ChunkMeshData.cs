using Godot;
using Hex;
using System.Collections.Generic;
using LibNoise.Primitive;
using LibNoise;

public class ChunkMeshData {
	private SimplexPerlin noise;

	public List<Vector3> vertices;
	public List<Vector2> uvs;
	public List<Vector3> normals;
	public List<Color> colors;
    public List<int> triangles;

	public ChunkMeshData() {
		this.noise = new SimplexPerlin(123, NoiseQuality.Best);
		Clear();
	}

	public void Clear() {
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
		colors = new List<Color>();
		triangles = new List<int>();
	}

	public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(Perturb(v1));
		vertices.Add(Perturb(v2));
		vertices.Add(Perturb(v3));
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	public void AddTriangleSubdivided(Vector3 v1, Vector3 v2, Vector3 v3, int subdivisions = 2) {
		if (subdivisions == 1) {
			int vertexIndex = vertices.Count;
			vertices.Add(Perturb(v1));
			vertices.Add(Perturb(v2));
			vertices.Add(Perturb(v3));
			triangles.Add(vertexIndex);
			triangles.Add(vertexIndex + 1);
			triangles.Add(vertexIndex + 2);
		} else {
			var v4 = v1.LinearInterpolate(v3, 0.5f);
			var v5 = v1.LinearInterpolate(v2, 0.5f);
			var v6 = v2.LinearInterpolate(v3, 0.5f);
			AddTriangleSubdivided(v1, v5, v4, subdivisions - 1);
			AddTriangleSubdivided(v4, v6, v3, subdivisions - 1);
			AddTriangleSubdivided(v4, v5, v6, subdivisions - 1);
			AddTriangleSubdivided(v5, v2, v6, subdivisions - 1);
		}
	}

	public void AddTriangleColor(Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	public void AddTriangleColor(Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add(Perturb(v1));
		vertices.Add(Perturb(v2));
		vertices.Add(Perturb(v3));
		vertices.Add(Perturb(v4));
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

	public void AddQuadColor(Color c1, Color c2) {
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}

	public void AddQuadColor(Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}
	
	public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
		uvs.Add(uv4);
	}

	public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
		uvs.Add(new Vector2(uMin, vMin));
		uvs.Add(new Vector2(uMax, vMin));
		uvs.Add(new Vector2(uMin, vMax));
		uvs.Add(new Vector2(uMax, vMax));
	}

	public void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color) {
		AddTriangle(center, edge.v1, edge.v2);
		AddTriangleColor(color);
		AddTriangle(center, edge.v2, edge.v3);
		AddTriangleColor(color);
		AddTriangle(center, edge.v3, edge.v4);
		AddTriangleColor(color);
		AddTriangle(center, edge.v4, edge.v5);
		AddTriangleColor(color);
		AddTriangle(center, edge.v5, edge.v6);
		AddTriangleColor(color);
		AddTriangle(center, edge.v6, edge.v7);
		AddTriangleColor(color);
		AddTriangle(center, edge.v7, edge.v8);
		AddTriangleColor(color);
		AddTriangle(center, edge.v8, edge.v9);
		AddTriangleColor(color);
	}

	// Used in rivers to create the outer half of the cell center
	public void TriangulateCenterOuter(HexCell cell, EdgeVertices edge, Direction dir, Color color) {
		// TODO: this is broken somehow
		var c1 = dir.CornerLeft();
		var c2 = dir.CornerRight();
		var c1_A = cell.GetCornerPoint(cell.Center, c1, CornerPoint.A);
		var c2_A = cell.GetCornerPoint(cell.Center, c2, CornerPoint.A);
		var c1_B = cell.GetCornerPoint(cell.Center, c1, CornerPoint.B);
		var c2_B = cell.GetCornerPoint(cell.Center, c2, CornerPoint.B);
		var c1_C = cell.GetCornerPoint(cell.Center, c1, CornerPoint.C);
		var c2_C = cell.GetCornerPoint(cell.Center, c2, CornerPoint.C);

		var S1 = cell.GetSidePoint(cell.Center, dir, SidePoint.S1);
		var S2 = cell.GetSidePoint(cell.Center, dir, SidePoint.S2);
		var S3 = cell.GetSidePoint(cell.Center, dir, SidePoint.S3);
		var S4 = cell.GetSidePoint(cell.Center, dir, SidePoint.S4);
		var S5 = cell.GetSidePoint(cell.Center, dir, SidePoint.S5);
		var S6 = cell.GetSidePoint(cell.Center, dir, SidePoint.S6);
		var C1 = cell.GetSidePoint(cell.Center, dir, SidePoint.C1);
		var C2 = cell.GetSidePoint(cell.Center, dir, SidePoint.C2);
		var C3 = cell.GetSidePoint(cell.Center, dir, SidePoint.C3);

		// triangles on both sides
		AddTriangle(edge.v1, edge.v2, c1_A);
		AddTriangleColor(color);
		AddTriangle(edge.v8, edge.v9, c2_A);
		AddTriangleColor(color);
		AddTriangle(c1_A, S3, c1_B);
		AddTriangleColor(color);
		AddTriangle(S4, c2_A, c2_B);
		AddTriangleColor(color);

		// quads in center
		AddQuad(c1_A, S3, edge.v2, edge.v3);
		AddQuadColor(color);
		AddQuad(S3, S5, edge.v3, edge.v4);
		AddQuadColor(color);
		AddQuad(S5, C1, edge.v4, edge.v5);
		AddQuadColor(color);
		AddQuad(C1, S6, edge.v5, edge.v6);
		AddQuadColor(color);
		AddQuad(S6, S4, edge.v6, edge.v7);
		AddQuadColor(color);
		AddQuad(S4, c2_A, edge.v7, edge.v8);
		AddQuadColor(color);

		AddQuad(c1_B, S1, S3, S5);
		AddQuadColor(color);
		AddQuad(S1, C2, S5, C1);
		AddQuadColor(color);
		AddQuad(C2, S2, C1, S6);
		AddQuadColor(color);
		AddQuad(S2, c2_B, S6, S4);
		AddQuadColor(color);
	}

	// TODO: use for hills and mountains
	public void TriangulateCenterInner(HexCell cell, Direction dir, Color color) {
		var side_points = dir.Points();
		var c1_points = dir.CornerLeft().Points();
		var c2_points = dir.CornerRight().Points();

		var c1 = dir.CornerLeft();
		var c2 = dir.CornerRight();
		var c1_A = cell.GetCornerPoint(cell.Center, c1, CornerPoint.A);
		var c2_A = cell.GetCornerPoint(cell.Center, c2, CornerPoint.A);
		var c1_B = cell.GetCornerPoint(cell.Center, c1, CornerPoint.B);
		var c2_B = cell.GetCornerPoint(cell.Center, c2, CornerPoint.B);
		var c1_C = cell.GetCornerPoint(cell.Center, c1, CornerPoint.C);
		var c2_C = cell.GetCornerPoint(cell.Center, c2, CornerPoint.C);

		var S1 = cell.GetSidePoint(cell.Center, dir, SidePoint.S1);
		var S2 = cell.GetSidePoint(cell.Center, dir, SidePoint.S2);
		var S3 = cell.GetSidePoint(cell.Center, dir, SidePoint.S3);
		var S4 = cell.GetSidePoint(cell.Center, dir, SidePoint.S4);
		var S5 = cell.GetSidePoint(cell.Center, dir, SidePoint.S5);
		var S6 = cell.GetSidePoint(cell.Center, dir, SidePoint.S6);
		var C1 = cell.GetSidePoint(cell.Center, dir, SidePoint.C1);
		var C2 = cell.GetSidePoint(cell.Center, dir, SidePoint.C2);
		var C3 = cell.GetSidePoint(cell.Center, dir, SidePoint.C3);

		// triangles on both sides
		AddTriangle(c1_B, S1, c1_C);
		AddTriangleColor(color);
		AddTriangle(S2, c2_B, c2_C);
		AddTriangleColor(color);
		AddTriangle(c1_C, C3, cell.Center);
		AddTriangleColor(color);
		AddTriangle(C3, c2_C, cell.Center);
		AddTriangleColor(color);

		// TODO: add quads
	}

	public void TriangulateEdgeStrip(
		EdgeVertices e1, Color c1,
		EdgeVertices e2, Color c2
	) {
		AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		AddQuadColor(c1, c2);
		AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
		AddQuadColor(c1, c2);
		AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
		AddQuadColor(c1, c2);
		AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
		AddQuadColor(c1, c2);
		AddQuad(e1.v5, e1.v6, e2.v5, e2.v6);
		AddQuadColor(c1, c2);
		AddQuad(e1.v6, e1.v7, e2.v6, e2.v7);
		AddQuadColor(c1, c2);
		AddQuad(e1.v7, e1.v8, e2.v7, e2.v8);
		AddQuadColor(c1, c2);
		AddQuad(e1.v8, e1.v9, e2.v8, e2.v9);
		AddQuadColor(c1, c2);
	}

	public void TriangulateEdgeStripRiver(
		EdgeVertices e1, Color c1,
		EdgeVertices e2, Color c2,
		Vector3 riverBank, Vector3 river
	) {
		AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		AddQuadColor(c1, c2);
		AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
		AddQuadColor(c1, c2);
		AddQuad(e1.v3, e1.v4 - river, e2.v3, e2.v4 - river);
		AddQuadColor(c1, c2);
		AddQuad(e1.v4 - river, e1.v5 - riverBank, e2.v4 - river, e2.v5 - riverBank);
		AddQuadColor(c1, c2);
		AddQuad(e1.v5 - riverBank, e1.v6 - river, e2.v5 - riverBank, e2.v6 - river);
		AddQuadColor(c1, c2);
		AddQuad(e1.v6 - river, e1.v7, e2.v6 - river, e2.v7);
		AddQuadColor(c1, c2);
		AddQuad(e1.v7, e1.v8, e2.v7, e2.v8);
		AddQuadColor(c1, c2);
		AddQuad(e1.v8, e1.v9, e2.v8, e2.v9);
		AddQuadColor(c1, c2);
	}

	const float perturbStrength = 1.0f;
	const float noiseScale = 0.05f;

    Vector3 Perturb(Vector3 position) {
		var x = noise.GetValue(Mathf.Round(position.x) * noiseScale, 0, Mathf.Round(position.z) * noiseScale);
		var z = noise.GetValue(Mathf.Round(position.x) * noiseScale, 0, Mathf.Round(position.z) * noiseScale);
		position.x += (x * 2f - 1f) * perturbStrength;
		position.z += (z * 2f - 1f) * perturbStrength;
		return position;
	}
}
