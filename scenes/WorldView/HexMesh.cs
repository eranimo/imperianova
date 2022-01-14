using Godot;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Primitive;
using System.Reactive.Subjects;

public class HexMesh : Spatial {
    private SimplexPerlin noise;

    private List<Vector3> vertices;
	private List<Vector2> uvs;
	private List<Vector3> normals;
	private List<Color> colors;
    private List<int> triangles;
    private readonly bool hasCollision;
    private readonly bool useUVCoordinates;
    public ShaderMaterial material;
	public Subject<Vector2> MeshClickPos = new Subject<Vector2>(); 
	public Subject<Vector2> MeshHoverPos = new Subject<Vector2>(); 

    public HexMesh(string name, ShaderMaterial material, bool hasCollision = false, bool useUVCoordinates = false) {
		this.Name = name;
		this.noise = new SimplexPerlin(123, NoiseQuality.Best);
		Clear();
        this.material = material;
        this.hasCollision = hasCollision;
        this.useUVCoordinates = useUVCoordinates;
    }

	public void Clear() {
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
		colors = new List<Color>();
		triangles = new List<int>();

		foreach (Node n in GetChildren()) {
			RemoveChild(n);
		}
	}

	public void GenerateMesh() {
		var indexArray = triangles.ToArray();
		var uvArray = uvs.ToArray();
		var colorArray = colors.ToArray();
		var vertexArray = vertices.ToArray();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Index] = indexArray;
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		if (useUVCoordinates) {
			arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		}
		if (colors.Count > 0) {
			arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;
		}

		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		// GD.PrintT(indexArray.Length, uvArray.Length, colorArray.Length, vertexArray.Length, normalArray.Length);
		mesh.SurfaceSetMaterial(0, material);

		var meshInstance = new MeshInstance();
		meshInstance.Name = $"{Name}MeshInstance";
		meshInstance.Mesh = mesh;
		AddChild(meshInstance);

		if (hasCollision) {
			var staticBody = new StaticBody();
			staticBody.Name = $"{Name}StaticBody";
			var collision = new CollisionShape();
			collision.Name = $"{Name}Collision";
			collision.Shape = mesh.CreateTrimeshShape();
			staticBody.AddChild(collision);
			staticBody.Connect("input_event", this, nameof(_handle_input));
			AddChild(staticBody);
		}
	}

	private void _handle_input(Camera camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx) {
		var o = this.GlobalTransform.origin;
		var hexPosition = new Vector2(o.x, o.z) + new Vector2(position.x, position.z) - Hex.HexUtils.HexCenter;
		var hex = Hex.HexUtils.PixelToHexOffset(hexPosition);

		if (@event.IsActionPressed("ui_select")) {
			MeshClickPos.OnNext(hexPosition);
		} else {
			MeshHoverPos.OnNext(hexPosition);
		}
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
