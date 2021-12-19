using Godot;
using System;
using LibNoise;
using System.Collections.Generic;

namespace Hex {
	public enum Direction {
		SE = 0,
		NE = 1,
		N = 2,
		NW = 3,
		SW = 4,
		S = 5,
	}

	public static class DirectionExtensions {
		public static Direction Next(this Direction dir) {
			return (Direction) ((((int) dir) + 1) % 6);
		}

		public static Direction Prev(this Direction dir) {
			return ((int) dir) == 0 ? Direction.S : (Direction) ((int) dir - 1);
		}
	}

	public enum HexCorner: int {
		E = 0,
		SE = 1,
		SW = 2,
		W = 3,
		NW = 4,
		NE = 5,
	}

	/// <summary>Offset coordinates in odd-q style</summary>
	public struct OffsetCoord {
		public int Col;
		public int Row;

		public OffsetCoord(int Col, int Row) {
			this.Col = Col;
			this.Row = Row;
		}
		
		public AxialCoord ToAxial() {
			var q = this.Col;
			var r = this.Row - (this.Col - (this.Col & 1)) / 2;
			return new AxialCoord(q, r);
		}

		public CubeCoord ToCube() {
			return this.ToAxial().ToCube();
		}

		public override string ToString() {
			return base.ToString() + string.Format("({0}, {1})", this.Col, this.Row);
		}

		public Vector2 AsVector() {
			return new Vector2(this.Col, this.Row);
		}

		public override bool Equals(object obj) {
			if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
				return false;
			}
			OffsetCoord other = (OffsetCoord) obj;
			return this.Row == other.Row && this.Col == other.Col;
		}

		public override int GetHashCode() {
			return (Col, Row).GetHashCode();
		}

		public static OffsetCoord operator +(OffsetCoord a, OffsetCoord b){
			return new OffsetCoord(a.Col + b.Col, a.Row + b.Row);
		}
	}

	public struct AxialCoord {
		public double q;
		public double r;

		public AxialCoord(double q, double r) {
			this.q = q;
			this.r = r;
		}

		public CubeCoord ToCube() {
			var q = this.q;
			var r = this.r;
			var s = -q - r;
			return new CubeCoord(q, r, s);
		}

		public OffsetCoord ToOffset() {
			var col = this.q;
			var row = this.r + (this.q - ((int) this.q & 1)) / 2;
			return new OffsetCoord((int) col, (int) row);
		}

		public AxialCoord Round() {
			return this.ToCube().Round().ToAxial();
		}

		public override string ToString() {
			return base.ToString() + string.Format("({0}, {1})", this.q, this.r);
		}

		public Vector2 AsVector() {
			return new Vector2((float) this.q, (float) this.r);
		}

		public override bool Equals(object obj) {
			if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
				return false;
			}
			AxialCoord other = (AxialCoord) obj;
			return this.q == other.q && this.r == other.r;
		}

		public override int GetHashCode() {
			return (q, r).GetHashCode();
		}

		public static AxialCoord operator +(AxialCoord a, AxialCoord b){
			return new AxialCoord(a.q + b.q, a.r + b.r);
		}
	}

	public struct CubeCoord {
		public double q;
		public double r;
		public double s;

		public CubeCoord(double q, double r, double s) {
			this.q = q;
			this.r = r;
			this.s = s;
		}

		public AxialCoord ToAxial() {
			var q = this.q;
			var r = this.r;
			return new AxialCoord(q, r);
		}

		public OffsetCoord ToOffset() {
			return this.ToAxial().ToOffset();
		}

		public CubeCoord Round() {
			var q = (int) Math.Round(this.q);
			var r = (int) Math.Round(this.r);
			var s = (int) Math.Round(this.s);

			var q_diff = Math.Abs(q - this.q);
			var r_diff = Math.Abs(r - this.r);
			var s_diff = Math.Abs(s - this.s);

			if (q_diff > r_diff && q_diff > s_diff) {
				q = -r - s;
			} else if(r_diff > s_diff) {
				r = -q - s;
			} else {
				s = -q - r;
			}

			return new CubeCoord(q, r, s);
		}

		public override string ToString() {
			return base.ToString() + string.Format("({0}, {1}, {2})", this.q, this.r, this.s);
		}

		public override bool Equals(object obj) {
			if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
				return false;
			}
			CubeCoord other = (CubeCoord) obj;
			return this.q == other.q && this.r == other.r && this.s == other.s;
		}

		public override int GetHashCode() {
			return (q, r, s).GetHashCode();
		}

		public static CubeCoord operator +(CubeCoord a, CubeCoord b){
			return new CubeCoord(a.q + b.q, a.r + b.r, a.s + b.s);
		}

		public CubeCoord Scale(double factor) {
			return new CubeCoord(this.q * factor, this.r * factor, this.s * factor);
		}
	}

	public static class HexConstants {
		public const int HEX_SIZE = 24;

		public static OffsetCoord[,] oddq_directions = new OffsetCoord[2, 6] {
			{
				new OffsetCoord(1,  0), new OffsetCoord(1, -1), new OffsetCoord(0, -1),
				new OffsetCoord(-1, -1), new OffsetCoord(-1,  0), new OffsetCoord(0, 1)
			},
			{
				new OffsetCoord(1, 1), new OffsetCoord(1, 0), new OffsetCoord( 0, -1),
				new OffsetCoord(-1, 0), new OffsetCoord(-1, 1), new OffsetCoord(0, 1),
			}
		};

		public static CubeCoord[] cube_directions = new CubeCoord[6] {
			new CubeCoord(1, 0, -1), new CubeCoord(1, -1, 0), new CubeCoord(0, -1, 1), 
			new CubeCoord(-1, 0, 1), new CubeCoord(-1, 1, 0), new CubeCoord(0, 1, -1), 
		};

		public static Dictionary<Direction, HexCorner[]> directionCorners = new Dictionary<Direction, HexCorner[]> {
			{ Direction.SE, new HexCorner[] { HexCorner.SE, HexCorner.E } },
			{ Direction.NE, new HexCorner[] { HexCorner.E, HexCorner.NE } },
			{ Direction.N, new HexCorner[] { HexCorner.NE, HexCorner.NW } },
			{ Direction.NW, new HexCorner[] { HexCorner.NW, HexCorner.W } },
			{ Direction.SW, new HexCorner[] { HexCorner.W, HexCorner.SW } },
			{ Direction.S, new HexCorner[] { HexCorner.SW, HexCorner.SE } },
		};

		public static Dictionary<Direction, Direction> oppositeDirections = new Dictionary<Direction, Direction> {
			{ Direction.SE, Direction.NW },
			{ Direction.NE, Direction.SW },
			{ Direction.N, Direction.S },
			{ Direction.NW, Direction.SE },
			{ Direction.SW, Direction.NE },
			{ Direction.S, Direction.N },
		};
	}

	public class HexUtils {
		public static float HexWidth {
			get {
				return 2 * HexConstants.HEX_SIZE;
			}
		}

		public static float HexHeight {
			get {
				return (float) Math.Sqrt(3) * HexConstants.HEX_SIZE;
			}
		}

		public static Vector2 HexCenter = new Vector2(HexUtils.HexWidth / 2f, HexUtils.HexHeight / 2f);

		public static Vector2 GetGridDimensions(int cols, int rows) {
			var lastHexPoint = Hex.HexUtils.HexToPixel(new OffsetCoord(cols - 1, rows - 1));
			var gridWidth = lastHexPoint.x + (HexUtils.HexWidth);
			float gridHeight;
			if ((rows & 2) == 0) {
				gridHeight = lastHexPoint.y + (HexUtils.HexHeight) + (HexUtils.HexHeight / 2);
			} else {
				gridHeight = lastHexPoint.y + (HexUtils.HexHeight);
			}
			return new Vector2(gridWidth, gridHeight);
		}

		public static OffsetCoord GetNeighbor(OffsetCoord hex, Direction direction) {
			var parity = hex.Col & 1;
			var dir = HexConstants.oddq_directions[parity, (int) direction];
			return new OffsetCoord(hex.Col + dir.Col, hex.Row + dir.Row);
		}

		public static CubeCoord GetNeighbor(CubeCoord hex, Direction direction) {
			return hex + HexConstants.cube_directions[(int) direction];
		}

		///<summary>Converts between pixels to offset coordinates</summary>
		public static OffsetCoord PixelToHexOffset(Vector2 point) {
			return HexUtils.PixelToHexAxial(point).ToOffset();
		}

		public static AxialCoord PixelToHexAxial(Vector2 point) {
			var q = (2.0 / 3 * point.x) / HexConstants.HEX_SIZE;
			var r = (-1.0 / 3 * point.x + Math.Sqrt(3) / 3 * point.y) / HexConstants.HEX_SIZE;
			return new AxialCoord(q, r).Round();
		}

		public static Vector2 HexToPixel(OffsetCoord hex) {
			var x = HexConstants.HEX_SIZE * 3/2 * hex.Col;
			var y = HexConstants.HEX_SIZE * Math.Sqrt(3) * (hex.Row + 0.5 * (hex.Col&1));
			return new Vector2((float) x, (float) y);
		}

		public static Vector2 HexToPixel(AxialCoord hex) {
			var x = HexConstants.HEX_SIZE * (3.0 / 2f * hex.q);
			var y = HexConstants.HEX_SIZE * (Math.Sqrt(3) / 2f * hex.q + Math.Sqrt(3) * hex.r);
			return new Vector2((float) x, (float) y);
		}

		public static Vector2 HexToPixelCenter(OffsetCoord hex) {
			return HexToPixel(hex) + HexUtils.HexCenter;
		}

		public static Vector2 HexToPixelCenter(AxialCoord hex) {
			return HexToPixel(hex) + HexUtils.HexCenter;
		}

		public static Vector2 GetHexCorner(double size, HexCorner corner) {
			double deg = 60f * (int) corner;
			double rad = (Math.PI / 180f) * deg;
			return new Vector2((float) (size * Math.Cos(rad)), (float) (size * Math.Sin(rad)));
		}

		public static List<OffsetCoord> GetRing(CubeCoord center, int radius = 1) {
			var results = new List<OffsetCoord>();
			var hex = center + new CubeCoord(-radius, radius, 0);
			for (int i = 0; i < 6; i++) {
				for (int j = 0; j < radius; j++) {
					results.Add(hex.ToOffset());
					hex = GetNeighbor(hex, (Direction) i);
				}
			}
			return results;
		}

		public static List<OffsetCoord> GetRing(OffsetCoord center, int radius = 1) {
			return GetRing(center.ToCube());
		}
	}
}
