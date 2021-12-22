using Godot;
using Godot.Collections;
using System;
using Hex;
using System.Collections.Generic;
using LibNoise;
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

public class HexMesh {
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

	public ArrayMesh GenerateMesh() {
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

		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		// GD.PrintT(indexArray.Length, uvArray.Length, colorArray.Length, vertexArray.Length, normalArray.Length);
		return mesh;
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

	const float perturbStrength = 0.85f;
	const float noiseScale = 0.1f;

	Vector3 Perturb(Vector3 position) {
		var x = noise.GetValue(position.x * noiseScale, 0, position.z * noiseScale);
		var z = noise.GetValue(position.x * noiseScale, 0, position.z * noiseScale);
		position.x += (x * 2f - 1f) * perturbStrength;
		position.z += (z * 2f - 1f) * perturbStrength;
		return position;
	}
}

public class MapChunk : StaticBody {
    private readonly HexGrid hexGrid;
    private readonly OffsetCoord firstHex;
    private readonly OffsetCoord chunkSize;
    private Random rng;

    private SimplexPerlin noise;
    private HexMesh terrain;
    private HexMesh water;

    public MapChunk(
		HexGrid hexGrid,
		OffsetCoord firstHex,
		OffsetCoord chunkSize
	) {
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

		var collision = new CollisionShape();
		collision.Name = "TerrainCollision";
		collision.Shape = terrainMesh.CreateTrimeshShape();
		AddChild(collision);

		var waterMesh = water.GenerateMesh();
		waterMesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/materials/Water.tres"));

		var waterMeshInstance = new MeshInstance();
		waterMeshInstance.Name = "Water";
		waterMeshInstance.Mesh = waterMesh;
		AddChild(waterMeshInstance);
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

		var neighbor_center = WithHeight(HexUtils.HexToPixelCenter(neighbor.Position), neighbor.WaterLevel);
		var v2_n = neighbor_center + dir.Opposite().CornerRight().InnerPosition();
		var v1_n = neighbor_center + dir.Opposite().CornerLeft().InnerPosition();
		EdgeVertices e2 = new EdgeVertices(v2_n, v1_n);
		water.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		water.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
		water.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
		water.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

		var prev_neighbor = cell.GetNeighbor(dir.Prev());
		if (prev_neighbor != null) {
			var prev_opp_dir = dir.Prev().Opposite();
			var prev_opp_center = WithHeight(HexUtils.HexToPixelCenter(prev_neighbor.Position), prev_neighbor.WaterLevel);
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
			var neighbor_center = WithHeight(HexUtils.HexToPixelCenter(neighbor.Position), neighbor.WaterLevel);
			var v2_n = neighbor_center + neighbor_dir.CornerRight().InnerPosition();
			var v1_n = neighbor_center + neighbor_dir.CornerLeft().InnerPosition();
			var neighbor_color = neighbor.color;

			water.AddQuad(v1, v2, v2_n, v1_n);

			// add corner
			var prev_neighbor = cell.GetNeighbor(dir.Prev());
			if (dir > 0 && prev_neighbor != null && prev_neighbor.IsUnderwater) {
				var prev_opp_dir = dir.Prev().Opposite();
				var prev_opp_center = WithHeight(HexUtils.HexToPixelCenter(prev_neighbor.Position), prev_neighbor.WaterLevel);
				var v2_prev_neighbor = prev_opp_center + prev_opp_dir.CornerRight().InnerPosition();

				water.AddTriangle(v2, v1_n, v2_prev_neighbor);
			}
		}
	}

	private void Triangulate(HexCell cell, Direction dir) {
		var d = (int) dir;
		var center = WithHeight(HexUtils.HexToPixelCenter(cell.Position), cell.Height);
		var v1 = center + dir.CornerLeft().InnerPosition();
		var v2 = center + dir.CornerRight().InnerPosition();
		var edge = new EdgeVertices(v1, v2);
		
		terrain.TriangulateEdgeFan(center, edge, cell.color);

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
		var neighbor_center = WithHeight(HexUtils.HexToPixelCenter(neighbor.Position), neighbor.Height);
		var v2_n = neighbor_center + neighbor_dir.CornerRight().InnerPosition();
		var v1_n = neighbor_center + neighbor_dir.CornerLeft().InnerPosition();
		var e2 = new EdgeVertices(v2_n, v1_n);

		terrain.TriangulateEdgeStrip(e1, cell.color, e2, neighbor.color);

		// corner
		var prev_neighbor = cell.GetNeighbor(dir.Prev());
		if (dir > 0 && prev_neighbor != null) {
			var prev_opp_dir = dir.Prev().Opposite();
			var prev_opp_center = WithHeight(HexUtils.HexToPixelCenter(prev_neighbor.Position), prev_neighbor.Height);
			var v2_prev_neighbor = prev_opp_center + prev_opp_dir.CornerRight().InnerPosition();

			terrain.AddTriangle(e1.v5, e2.v5, v2_prev_neighbor);
			terrain.AddTriangleColor(cell.color, neighbor.color, prev_neighbor.color);
		}
	}

	Vector3 WithHeight(Vector2 vector, double height) {
		return new Vector3(vector.x, (float) height, vector.y);
	}
}
