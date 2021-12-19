using Godot;
using Godot.Collections;
using System;
using Hex;
using System.Collections.Generic;

public class MapChunk : StaticBody {
    private readonly HexGrid hexGrid;
    private readonly OffsetCoord firstHex;
    private readonly OffsetCoord chunkSize;
    private Random rng;

	private Vector2[] hexCorners = new Vector2[] {
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.E),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.SE),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.SW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.W),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.NW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.NE),
	};
	private const double innerPercent = 0.75;
	private const double edgePercent = 1 - innerPercent;

	private Vector2[] hexInnerCorners = new Vector2[] {
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.E),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.SE),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.SW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.W),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.NW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.NE),
	};
    private Vector2[] uvArray;
    private Vector3[] vertexArray;
    private Color[] colorArray;
    private Vector3[] normalArray;
	private List<Vector3> vertices;
	private List<Vector2> uvs;
	private List<Vector3> normals;
	private List<Color> colors;

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
		Generate();
		// GD.PrintS($"Chunk generate: {watch.ElapsedMilliseconds}ms");
	}

	public void Generate() {
		generateMesh();
	}

	private void generateMesh() {
		var mesh = new ArrayMesh();
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
		colors = new List<Color>();

		// int i = 0;
		for (int col = 0; col < chunkSize.Col; col++) {
			for (int row = 0; row < chunkSize.Row; row++) {
				var hex = firstHex + new Hex.OffsetCoord(col, row);
				generateHex(hex);
			}
		}
		// GD.PrintS($"Generated {vertices.Count} vertices");

		foreach (Vector3 vertex in vertices) {
			uvs.Add(new Vector2(vertex.x, vertex.z));
		}

		this.uvArray = uvs.ToArray();
		this.vertexArray = vertices.ToArray();
		this.colorArray = colors.ToArray();

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

		this.normalArray = normals.ToArray();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;
		arrays[(int) ArrayMesh.ArrayType.Normal] = normalArray;

		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		// GD.PrintS("Total surfaces:", mesh.GetSurfaceCount());
		mesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/GridShader.tres"));

		var origin = HexUtils.HexToPixel(firstHex);
		Transform.Translated(new Vector3(origin.x, 0, origin.y));
		var meshInstance = new MeshInstance();
		meshInstance.Mesh = mesh;
		AddChild(meshInstance);
		var collision = new CollisionShape();
		collision.Shape = mesh.CreateTrimeshShape();
		AddChild(collision);
	}

	private void generateHex(Hex.OffsetCoord hex) {
		var cell = hexGrid.GetCell(hex);
		var center = HexUtils.HexToPixelCenter(hex);
		var color = cell.color;

		for (int d = 0; d < 6; d++) {
			var dir = (Direction) d;
			int c1 = (int) HexConstants.directionCorners[dir][1];
			int c2 = (int) HexConstants.directionCorners[dir][0];
			var v3 = center + hexCorners[c1];
			var v4 = center + hexCorners[c2];
			var edge_center = (v3 + v4) / 2f;
			var v1 = center + hexInnerCorners[c1];
			var v2 = center + hexInnerCorners[c2];
			var h = cell.Height;
			
			// center triangle
			AddTriangle(WithHeight(center, h), WithHeight(v1, h), WithHeight(v2, h));
			AddTriangleColor(color, color, color);

			if (d <= 2) {
				var neighbor = cell.GetNeighbor(dir);
				if (neighbor == null) {
					continue;
				}
				var avg_height = (h + neighbor.Height) / 2f;
				var prev_neighbor = cell.GetNeighbor(dir.Prev());
				var next_neighbor = cell.GetNeighbor(dir.Next());
				var neighbor_dir = dir.Opposite();
				var neighbor_center = HexUtils.HexToPixelCenter(neighbor.Position);
				int c1_n = (int) HexConstants.directionCorners[neighbor_dir][1];
				int c2_n = (int) HexConstants.directionCorners[neighbor_dir][0];
				var v1_n = neighbor_center + hexInnerCorners[c1_n];
				var v2_n = neighbor_center + hexInnerCorners[c2_n];
				var neighbor_color = neighbor.color;
				var h_n = neighbor.Height;

				// edge 1
				AddTriangle(WithHeight(v1, h), WithHeight(v2_n, h_n), WithHeight(v2, h));
				AddTriangleColor(color, neighbor_color, color);

				// edge 2
				AddTriangle(WithHeight(v2, h), WithHeight(v2_n, h_n), WithHeight(v1_n, h_n));
				AddTriangleColor(color, neighbor_color, neighbor_color);

				// corner
				if (dir > 0 && prev_neighbor != null) {
					var c2_prev_opp = (int) HexConstants.directionCorners[dir.Prev().Opposite()][0];
					var prev_opp_center = HexUtils.HexToPixelCenter(prev_neighbor.Position);
					var v2_prev_neighbor = prev_opp_center + hexInnerCorners[c2_prev_opp];
					var prev_opp = HexUtils.GetNeighbor(prev_neighbor.Position, dir.Prev().Opposite());
					var prev_opp_height = prev_neighbor.Height;
					var prev_opp_color = prev_neighbor.color;

					AddTriangle(WithHeight(v2, h), WithHeight(v1_n, h_n), WithHeight(v2_prev_neighbor, prev_opp_height));
					AddTriangleColor(color, neighbor_color, prev_opp_color);
				}
			}
		}
	}

	Vector3 WithHeight(Vector2 vector, double height) {
		return new Vector3(vector.x, (float) height, vector.y);
	}

	void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
	}

	void AddTriangleColor(Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}
}
