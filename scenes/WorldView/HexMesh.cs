using Godot;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Primitive;

public class HexMesh {
	public ArrayMesh mesh;

    private SimplexPerlin noise;

    private List<Vector3> vertices;
	private List<Vector2> uvs;
	private List<Vector3> normals;
	private List<Color> colors;
    private List<int> triangles;

    public HexMesh() {
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

	public void GenerateMesh() {
		foreach (Vector3 vertex in vertices) {
			uvs.Add(new Vector2(vertex.x, vertex.z));
		}

		var indexArray = triangles.ToArray();
		var uvArray = uvs.ToArray();
		var colorArray = colors.ToArray();
		var vertexArray = vertices.ToArray();

		// calculate normals
		var normalArray = new Vector3[vertices.Count];
		for (int i = 0; i < triangles.Count; i += 3) {
			var i1 = triangles[i];
			var i2 = triangles[i + 1];
			var i3 = triangles[i + 2];
			var v1 = vertices[i1];
			var v2 = vertices[i2];
			var v3 = vertices[i3];
			var U = v2 - v1;
			var V = v3 - v1;
			var normal = U.Cross(V);
			normalArray[i1] = normal;
			normalArray[i2] = normal;
			normalArray[i3] = normal;
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Index] = indexArray;
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		if (colors.Count > 0) {
			arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;
		}
		arrays[(int) ArrayMesh.ArrayType.Normal] = normalArray;

		mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		// GD.PrintT(indexArray.Length, uvArray.Length, colorArray.Length, vertexArray.Length, normalArray.Length);
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

	public void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color) {
		AddTriangle(center, edge.v1, edge.v2);
		AddTriangleColor(color);
		AddTriangle(center, edge.v2, edge.v3);
		AddTriangleColor(color);
		AddTriangle(center, edge.v3, edge.v4);
		AddTriangleColor(color);
		AddTriangle(center, edge.v4, edge.v5);
		AddTriangleColor(color);
	}

	// Used in rivers to create the outer half of the cell center
	public void TriangulateInnerEdgeFan(EdgeVertices edge, Vector3 b1, Vector3 b2, Color color) {
		var center = b1.LinearInterpolate(b2, 0.5f);
		AddTriangle(edge.v1, edge.v2, b1);
		AddTriangleColor(color);
		AddTriangle(edge.v2, edge.v3, b1);
		AddTriangleColor(color);
		AddTriangle(edge.v3, center, b1);
		AddTriangleColor(color);
		AddTriangle(b2, center, edge.v3);
		AddTriangleColor(color);
		AddTriangle(edge.v3, edge.v4, b2);
		AddTriangleColor(color);
		AddTriangle(edge.v4, edge.v5, b2);
		AddTriangleColor(color);
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
	}

	const float perturbStrength = 0.55f;
	const float noiseScale = 0.1f;

	Vector3 Perturb(Vector3 position) {
		// var x = noise.GetValue(Mathf.Round(position.x * noiseScale), 0, Mathf.Round(position.z * noiseScale));
		// var z = noise.GetValue(Mathf.Round(position.x * noiseScale), 0, Mathf.Round(position.z * noiseScale));
		// position.x += (x * 2f - 1f) * perturbStrength;
		// position.z += (z * 2f - 1f) * perturbStrength;
		return position;
	}
}
