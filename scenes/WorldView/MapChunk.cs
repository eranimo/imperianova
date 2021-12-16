using Godot;
using System;
using Hex;

public class MapChunk : MeshInstance {
	private readonly int width;
	private readonly int height;

	public MapChunk(
		int width,
		int height
	) {
		this.width = width;
		this.height = height;
	}

	public override void _Ready() {
		generateMesh();
	}

	private void generateMesh() {
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		for (int col = 0; col < width; col++) {
			for (int row = 0; row < height; row++) {
				generateHex(st, new Hex.OffsetCoord(col, row));
			}
		}

		st.Index();

		this.Mesh = st.Commit();
	}

	private void generateHex(SurfaceTool st, Hex.OffsetCoord hex) {
		GD.PrintS("Generating hex", hex);
		var origin = HexUtils.HexToPixel(hex);
		var center = origin + HexUtils.HexCenter;
		for (int c = 0; c < 6; c++) {
			int c_next = (c + 1) % 6;
			var c1 = HexUtils.GetHexCorner(center, HexConstants.HEX_SIZE, (HexCorner) c);
			var c2 = HexUtils.GetHexCorner(center, HexConstants.HEX_SIZE, (HexCorner) c_next);

			st.AddUv(center);
			st.AddVertex(new Vector3(center.x, 0, center.y));

			st.AddUv(c1);
			st.AddVertex(new Vector3(c1.x, 0, c1.y));

			st.AddUv(c2);
			st.AddVertex(new Vector3(c2.x, 0, c2.y));
		}
	}
}
