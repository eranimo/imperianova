using Godot;
using Godot.Collections;
using System;
using Hex;

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

		int verticesPerHex = 6 * 3 * 5;
		int arraySize = chunkSize.Col * chunkSize.Row * verticesPerHex;
		this.uvArray = new Vector2[arraySize];
		this.vertexArray = new Vector3[arraySize];
		this.colorArray = new Color[arraySize];
		this.normalArray = new Vector3[arraySize];

		int i = 0;
		for (int col = 0; col < chunkSize.Col; col++) {
			for (int row = 0; row < chunkSize.Row; row++) {
				var hex = firstHex + new Hex.OffsetCoord(col, row);
				generateHex(hex, i);
				i += verticesPerHex;
			}
		}

		// calculate normals
		for (int v = 0; v < arraySize; v += 3) {
			var p1 = vertexArray[v];
			var p2 = vertexArray[v + 1];
			var p3 = vertexArray[v + 2];
			var U = p2 - p1;
			var V = p3 - p1;
			var normal = U.Cross(V);
			normalArray[v] = normal;
			normalArray[v + 1] = normal;
			normalArray[v + 2] = normal;
		}

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

	private void generateHex(Hex.OffsetCoord hex, int index) {
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
			int i = index + (d * 3 * 5);
			var cell_height = cell.Height;
			
			// center triangle
			AddVertex(i, center, cell_height, color);
			AddVertex(i + 1, v1, cell_height, color);
			AddVertex(i + 2, v2, cell_height, color);

			if (d <= 2) {
				var neighbor = cell.GetNeighbor(dir);
				if (neighbor == null) {
					continue;
				}
				var avg_height = (cell_height + neighbor.Height) / 2f;
				var prev_neighbor = cell.GetNeighbor(dir.Prev());
				var next_neighbor = cell.GetNeighbor(dir.Next());
				var neighbor_dir = dir.Opposite();
				var neighbor_center = HexUtils.HexToPixelCenter(neighbor.Position);
				int c1_n = (int) HexConstants.directionCorners[neighbor_dir][1];
				int c2_n = (int) HexConstants.directionCorners[neighbor_dir][0];
				var v1_n = neighbor_center + hexInnerCorners[c1_n];
				var v2_n = neighbor_center + hexInnerCorners[c2_n];
				var neighbor_color = neighbor.color;

				// edge center 1
				AddVertex(i + 3, v1, cell_height, color);
				AddVertex(i + 4, v2_n, neighbor.Height, neighbor_color);
				AddVertex(i + 5, v2, cell_height, color);

				// edge center 2
				AddVertex(i + 6, v2, cell_height, color);
				AddVertex(i + 7, v2_n, neighbor.Height, neighbor_color);
				AddVertex(i + 8, v1_n, neighbor.Height, neighbor_color);

				if (dir > 0 && prev_neighbor != null) {
					var c2_prev_opp = (int) HexConstants.directionCorners[dir.Prev().Opposite()][0];
					var prev_opp_center = HexUtils.HexToPixelCenter(prev_neighbor.Position);
					var v2_prev_neighbor = prev_opp_center + hexInnerCorners[c2_prev_opp];
					var prev_opp = HexUtils.GetNeighbor(prev_neighbor.Position, dir.Prev().Opposite());
					var pre_opp_height = prev_neighbor.Height;
					var prev_opp_color = prev_neighbor.color;
					AddVertex(i + 9, v2, cell_height, color);
					AddVertex(i + 10, v1_n, neighbor.Height, neighbor_color);
					AddVertex(i + 11, v2_prev_neighbor, pre_opp_height, prev_opp_color);
				}
			}
		}
	}

	private void AddVertex(int index, Vector2 pos, double depth, Color color) {
		vertexArray[index] = new Vector3(pos.x, (float) depth, pos.y);
		uvArray[index] = new Vector2(pos.x, pos.y);
		colorArray[index] = color;
	}
}
