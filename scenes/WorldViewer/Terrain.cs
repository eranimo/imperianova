using Godot;
using System;

public enum HexCorner: int {
	EAST = 0,
	SOUTH_EAST = 1,
	SOUTH_WEST = 2,
	WEST = 3,
	NORTH_WEST = 4,
	NORTH_EAST = 5,
}

public class Hex {
	int size;
	Vector2 center;

	public Hex(Vector2 center_, int size_) {
		center = center_;
		size = size_;
	}

	public Vector2 get_hex_corner(HexCorner edge) {
		var angle_deg = 60 * (int) edge;
		var angle_rad = Math.PI / 180 * angle_deg;
		return new Vector2(
			center.x + size * (float) Math.Cos(angle_rad),
			center.y + size * (float) Math.Sin(angle_rad)
		);
	}
}

public class Terrain : MeshInstance {
	public override void _Ready() {
		GD.Print("Generate hex");
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		var center = new Vector2(25, 25);
		var hex = new Hex(center, 25);
		
		for (int c = 0; c <= 5; c++) {
			var corner = (HexCorner) c;
			var corner_point = hex.get_hex_corner(corner);
			var next_corner = (HexCorner) ((c + 1) % 6);
			var next_corner_point = hex.get_hex_corner(next_corner);

			st.AddSmoothGroup(true);
			st.AddUv(center);
			st.AddVertex(new Vector3(center.x, 25, center.y));

			st.AddSmoothGroup(true);
			st.AddUv(corner_point);
			st.AddVertex(new Vector3(corner_point.x, 0, corner_point.y));

			st.AddSmoothGroup(true);
			st.AddUv(next_corner_point);
			st.AddVertex(new Vector3(next_corner_point.x, 0, next_corner_point.y));
		}
		
		//  Create indices, indices are optional.
		st.Index();
		st.GenerateNormals();

		// Commit to a mesh.
		this.Mesh = st.Commit();
	}
}
