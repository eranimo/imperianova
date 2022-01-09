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

	/// <summary>Does this cell have a river going in from or going out to this direction?</summary>
	public bool HasRiver(Direction dir) {
		return IncomingRivers.Contains(dir) || OutgoingRivers.Contains(dir);
	}

	/// <summary>Does this cell have a river between dir1 to dir2 or from dir2 to dir1?</summary>
	public bool HasRiverFlowEither(Direction dir1, Direction dir2) {
		return HasRiverFlow(dir1, dir2) || HasRiverFlow(dir2, dir1);
	}

	/// <summary>Does this cell have a river flowing from dir1 to dir2?</summary>
	public bool HasRiverFlow(Direction dir1, Direction dir2) {
		return IncomingRivers.Contains(dir1) && OutgoingRivers.Contains(dir2);
	}

	/// <summary>Does this cell have rivers flowing in from these two directions?</summary>
	public bool HasRiverCounterFlow(Direction dir1, Direction dir2) {
		return (
			(IncomingRivers.Contains(dir1) && IncomingRivers.Contains(dir2)) ||
			(OutgoingRivers.Contains(dir1) && OutgoingRivers.Contains(dir2))
		);
	}
}
