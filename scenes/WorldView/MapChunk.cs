using Godot;
using Godot.Collections;
using System;
using Hex;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Primitive;

public class HexMesh {
    private SimplexPerlin noise;

    private List<Vector3> vertices;
	private List<Vector2> uvs;
	private List<Vector3> normals;
	private List<Color> colors;

	public HexMesh() {
		this.noise = new SimplexPerlin(123, NoiseQuality.Best);
		Clear();
	}

	public void Clear() {
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
		colors = new List<Color>();
	}

	public ArrayMesh GenerateMesh() {
		foreach (Vector3 vertex in vertices) {
			uvs.Add(new Vector2(vertex.x, vertex.z));
		}

		var uvArray = uvs.ToArray();
		var vertexArray = vertices.ToArray();
		var colorArray = colors.ToArray();

		// calculate normals
		for (int v = 0; v < vertices.Count; v += 3) {
			var p1 = vertexArray[v];
			var p2 = vertexArray[v + 1];
			var p3 = vertexArray[v + 2];
			var U = p2 - p1;
			var V = p3 - p1;
			var normal = U.Cross(V);
			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);
		}

		var normalArray = normals.ToArray();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		if (colors.Count > 0) {
			arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;
		}
		arrays[(int) ArrayMesh.ArrayType.Normal] = normalArray;

		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		// GD.PrintS($"Generated {vertices.Count} vertices");
		return mesh;
	}

	public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(Perturb(v1));
		vertices.Add(Perturb(v2));
		vertices.Add(Perturb(v3));
	}

	public void AddTriangleColor(Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	Vector3 Perturb(Vector3 position) {
		var x = noise.GetValue(position.x, 0, position.z);
		var z = noise.GetValue(position.x, 0, position.z);
		position.x += (x * 2f - 1f) * 1.5f;
		position.z += (z * 2f - 1f) * 1.5f;
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
					triangulateHex(cell, dir);

					if (cell.IsUnderwater) {
						triangulateWater(cell, dir);
					}
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

	private void triangulateWater(HexCell cell, Direction dir) {
		var center = WithHeight(HexUtils.HexToPixelCenter(cell.Position), cell.WaterLevel);
		var v1 = center + dir.CornerLeft().InnerPosition();
		var v2 = center + dir.CornerRight().InnerPosition();
		water.AddTriangle(center, v1, v2);

		var d = (int) dir;
		if (d <= 2) {
			var neighbor = cell.GetNeighbor(dir);
			if (neighbor == null || !neighbor.IsUnderwater) {
				return;
			}
			var neighbor_dir = dir.Opposite();
			var neighbor_center = WithHeight(HexUtils.HexToPixelCenter(neighbor.Position), neighbor.WaterLevel);
			var v1_n = neighbor_center + neighbor_dir.CornerLeft().InnerPosition();
			var v2_n = neighbor_center + neighbor_dir.CornerRight().InnerPosition();
			var neighbor_color = neighbor.color;

			// edge 1
			water.AddTriangle(v1, v2_n, v2);

			// edge 2
			water.AddTriangle(v2, v2_n, v1_n);

			var prev_neighbor = cell.GetNeighbor(dir.Prev());
			if (dir > 0 && prev_neighbor != null && prev_neighbor.IsUnderwater) {
				var prev_opp_dir = dir.Prev().Opposite();
				var prev_opp_center = WithHeight(HexUtils.HexToPixelCenter(prev_neighbor.Position), prev_neighbor.WaterLevel);
				var v2_prev_neighbor = prev_opp_center + prev_opp_dir.CornerRight().InnerPosition();

				water.AddTriangle(v2, v1_n, v2_prev_neighbor);
			}
		}
	}

	private void triangulateHex(HexCell cell, Direction dir) {
		var d = (int) dir;
		var center = WithHeight(HexUtils.HexToPixelCenter(cell.Position), cell.Height);
		var color = cell.color;
		var v1 = center + dir.CornerLeft().InnerPosition();
		var v2 = center + dir.CornerRight().InnerPosition();
		
		// center triangle
		terrain.AddTriangle(center, v1, v2);
		terrain.AddTriangleColor(color, color, color);

		if (d <= 2) {
			var neighbor = cell.GetNeighbor(dir);
			if (neighbor == null) {
				return;
			}
			var neighbor_dir = dir.Opposite();
			var neighbor_center = WithHeight(HexUtils.HexToPixelCenter(neighbor.Position), neighbor.Height);
			var v1_n = neighbor_center + neighbor_dir.CornerLeft().InnerPosition();
			var v2_n = neighbor_center + neighbor_dir.CornerRight().InnerPosition();
			var neighbor_color = neighbor.color;

			// edge 1
			terrain.AddTriangle(v1, v2_n, v2);
			terrain.AddTriangleColor(color, neighbor_color, color);

			// edge 2
			terrain.AddTriangle(v2, v2_n, v1_n);
			terrain.AddTriangleColor(color, neighbor_color, neighbor_color);

			// corner
			var prev_neighbor = cell.GetNeighbor(dir.Prev());
			if (dir > 0 && prev_neighbor != null) {
				var prev_opp_dir = dir.Prev().Opposite();
				var prev_opp_center = WithHeight(HexUtils.HexToPixelCenter(prev_neighbor.Position), prev_neighbor.Height);
				var v2_prev_neighbor = prev_opp_center + prev_opp_dir.CornerRight().InnerPosition();

				terrain.AddTriangle(v2, v1_n, v2_prev_neighbor);
				terrain.AddTriangleColor(color, neighbor_color, prev_neighbor.color);
			}
		}
	}

	Vector3 WithHeight(Vector2 vector, double height) {
		return new Vector3(vector.x, (float) height, vector.y);
	}
}
