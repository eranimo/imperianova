using Godot;
using Hex;
using System.Collections.Generic;

public class HexCell {
	public OffsetCoord Position;
	public HexGrid Grid;

	public double Height;
	public double WaterLevel;
	public Color Color;
	public HashSet<Direction> IncomingRivers = new HashSet<Direction>();
	public HashSet<Direction> OutgoingRivers = new HashSet<Direction>();

	public HexCell(OffsetCoord position) {
		Position = position;
	}

	public Vector3 Center {
		get {
			var pos = HexUtils.HexToPixelCenter(Position);
			return new Vector3(pos.x, (float) Height, pos.y);
		}
	}

	public Vector3 WaterCenter {
		get {
			var pos = HexUtils.HexToPixelCenter(Position);
			return new Vector3(pos.x, (float) WaterLevel, pos.y);
		}
	}

	public HexCell GetNeighbor(Direction dir) {
		var pos = HexUtils.GetNeighbor(this.Position, dir);
		return Grid.GetCell(pos);
	}

	public Direction? GetDirectionOfNeighbor(HexCell cell) {
		for(int dir = 0; dir < 6; dir++) {
			if (GetNeighbor((Direction) dir) == cell) {
				return (Direction) dir;
			}
		}
		return null;
	}

	public bool IsUnderwater {
		get {
			return WaterLevel > Height;
		}
	}

	public bool HasRiver() {
		return IncomingRivers.Count > 0 || OutgoingRivers.Count > 0;
	}

	public bool HasRiver(Direction dir) {
		return IncomingRivers.Contains(dir) || OutgoingRivers.Contains(dir);
	}
}
