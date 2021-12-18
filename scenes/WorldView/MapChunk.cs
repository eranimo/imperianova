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
	private const double edgePercent = 0.75;
	private Vector2[] hexEdgeCorners = new Vector2[] {
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * edgePercent, HexCorner.E),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * edgePercent, HexCorner.SE),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * edgePercent, HexCorner.SW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * edgePercent, HexCorner.W),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * edgePercent, HexCorner.NW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE * edgePercent, HexCorner.NE),
	};

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

		int triangles = chunkSize.Col * chunkSize.Row * 6 * 3;
		var uvArray = new Vector2[triangles];
		var vertexArray = new Vector3[triangles];
		var colorArray = new Color[triangles];

		int i = 0;
		for (int col = 0; col < chunkSize.Col; col++) {
			for (int row = 0; row < chunkSize.Row; row++) {
				var hex = firstHex + new Hex.OffsetCoord(col, row);
				generateHex(hex, i, uvArray, vertexArray, colorArray);
				i += 6 * 3;
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

	private void generateHex(Hex.OffsetCoord hex, int index, Vector2[] uvArray, Vector3[] vertexArray, Color[] colorArray) {
		var cell = hexGrid.GetCell(hex);
		var origin = HexUtils.HexToPixel(hex);
		var center = origin + HexUtils.HexCenter;
		var color = cell.color;

		for (int d = 0; d < 6; d++) {
			var dir = (Direction) d;
			var neighbor = cell.GetNeighbor(dir);
			int c = (int) HexConstants.directionCorners[dir][1];
			int c_next = (int) HexConstants.directionCorners[dir][0];
			var v1 = center + hexCorners[c];
			var v2 = center + hexCorners[c_next];
			var v3 = center + hexEdgeCorners[c];
			var v4 = center + hexEdgeCorners[c_next];
			int i = index + (c * 3);

			vertexArray[i] = new Vector3(center.x, 0, center.y);
			uvArray[i] = new Vector2(center.x, center.y);
			colorArray[i] = color;

			vertexArray[i + 1] = new Vector3(v1.x, 0, v1.y);
			uvArray[i + 1] = new Vector2(v1.x, v1.y);
			colorArray[i + 1] = neighbor != null ? neighbor.color : color;

			vertexArray[i + 2] = new Vector3(v2.x, 0, v2.y);
			uvArray[i + 2] = new Vector2(v2.x, v2.y);
			colorArray[i + 2] = neighbor != null ? neighbor.color : color;
		}
	}
}
