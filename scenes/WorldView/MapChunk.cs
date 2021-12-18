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
		rng = new Random();
		generateMesh();
	}

	private void generateMesh() {
		var mesh = new ArrayMesh();

		int verticesPerHex = 6 * 3 * 5;
		int arraySize = chunkSize.Col * chunkSize.Row * verticesPerHex;
		this.uvArray = new Vector2[arraySize];
		this.vertexArray = new Vector3[arraySize];
		this.colorArray = new Color[arraySize];

		int i = 0;
		for (int col = 0; col < chunkSize.Col; col++) {
			for (int row = 0; row < chunkSize.Row; row++) {
				var hex = firstHex + new Hex.OffsetCoord(col, row);
				generateHex(hex, i);
				i += verticesPerHex;
			}
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;

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
		var origin = HexUtils.HexToPixel(hex);
		var center = origin + HexUtils.HexCenter;
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
			var v5 = edge_center.LinearInterpolate(v3, (float) innerPercent);
			var v6 = edge_center.LinearInterpolate(v4, (float) innerPercent);
			int i = index + (d * 3 * 5);
			var neighbor = cell.GetNeighbor(dir);
			var prev_neighbor = cell.GetNeighbor((dir == 0 ? (Direction) 5 : (Direction) dir - 1));
			var next_neighbor = cell.GetNeighbor((Direction) (((int) dir + 1) % 6));
			var neighbor_color = neighbor != null ? neighbor.color : color;
			var prev_neighbor_color = prev_neighbor != null ? prev_neighbor.color : color;
			var next_neighbor_color = next_neighbor != null ? next_neighbor.color : color;
			prev_neighbor_color = (prev_neighbor_color + color + neighbor_color) / 3f;
			next_neighbor_color = (next_neighbor_color + color + neighbor_color) / 3f;
			neighbor_color = (neighbor_color + color) / 2f;
			
			// center triangle
			AddVertex(i, center, 0, color);
			AddVertex(i + 1, v1, 0, color);
			AddVertex(i + 2, v2, 0, color);
			
			// edge center 1
			AddVertex(i + 3, v1, 0, color);
			AddVertex(i + 4, v5, 0, neighbor_color);
			AddVertex(i + 5, v2, 0, color);

			// edge center 2
			AddVertex(i + 6, v2, 0, color);
			AddVertex(i + 7, v5, 0, neighbor_color);
			AddVertex(i + 8, v6, 0, neighbor_color);

			// // corner 1
			AddVertex(i + 9, v1, 0, color);
			AddVertex(i + 10, v3, 0, next_neighbor_color);
			AddVertex(i + 11, v5, 0, neighbor_color);

			// corner 2
			AddVertex(i + 12, v2, 0, color);
			AddVertex(i + 13, v6, 0, neighbor_color);
			AddVertex(i + 14, v4, 0, prev_neighbor_color);
		}
	}

	private void AddVertex(int index, Vector2 pos, float depth, Color color) {
		vertexArray[index] = new Vector3(pos.x, depth, pos.y);
		uvArray[index] = new Vector2(pos.x, pos.y);
		colorArray[index] = color;
	}
}
