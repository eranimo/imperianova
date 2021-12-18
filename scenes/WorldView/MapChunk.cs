using Godot;
using Godot.Collections;
using System;
using Hex;

public class MapChunk : StaticBody {
	private readonly int width;
	private readonly int height;
	private Random rng;

	private Vector2[] hexCorners = new Vector2[] {
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.E),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.NE),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.NW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.W),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.SW),
		HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.SE),
	};

	public MapChunk(
		int width,
		int height
	) {
		this.width = width;
		this.height = height;
	}

	public override void _Ready() {
		rng = new Random();
		generateMesh();
	}

	private void generateMesh() {
		var mesh = new ArrayMesh();

		int triangles = width * height * 6 * 3;
		var uvArray = new Vector2[triangles];
		var vertexArray = new Vector3[triangles];
		var colorArray = new Color[triangles];

		int i = 0;
		for (int col = 0; col < width; col++) {
			for (int row = 0; row < height; row++) {
				generateHex(new Hex.OffsetCoord(col, row), i, uvArray, vertexArray, colorArray);
				i += 6 * 3;
			}
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;

		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		GD.PrintS("Total surfaces:", mesh.GetSurfaceCount());
		mesh.SurfaceSetMaterial(0, (Material) ResourceLoader.Load("res://scenes/WorldView/GridShader.tres"));
		var meshInstance = new MeshInstance();
		meshInstance.Mesh = mesh;
		AddChild(meshInstance);
		var collision = new CollisionShape();
		collision.Shape = mesh.CreateTrimeshShape();
		AddChild(collision);
	}

	private void generateHex(Hex.OffsetCoord hex, int index, Vector2[] uvArray, Vector3[] vertexArray, Color[] colorArray) {
		var origin = HexUtils.HexToPixel(hex);
		var center = origin + HexUtils.HexCenter;
		var color = new Color((float) rng.NextDouble(), (float) rng.NextDouble(), (float) rng.NextDouble());

		for (int c = 0; c < 6; c++) {
			int c_next = (c + 1) % 6;
			var c1 = center + hexCorners[c];
			var c2 = center + hexCorners[c_next];
			int i = index + (c * 3);

			vertexArray[i] = new Vector3(center.x, 0, center.y);
			uvArray[i] = new Vector2(center.x, center.y);
			colorArray[i] = color;

			vertexArray[i + 1] = new Vector3(c1.x, 0, c1.y);
			uvArray[i + 1] = new Vector2(c1.x, c1.y);
			colorArray[i + 1] = color;

			vertexArray[i + 2] = new Vector3(c2.x, 0, c2.y);
			uvArray[i + 2] = new Vector2(c2.x, c2.y);
			colorArray[i + 2] = color;
		}
	}
}
