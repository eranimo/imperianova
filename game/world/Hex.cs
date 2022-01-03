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

		public static Direction Opposite(this Direction dir) {
			return HexConstants.oppositeDirections[dir];
		}

		public static HexCorner CornerLeft(this Direction dir) {
			return HexConstants.directionCorners[dir][0];
		}

		public static HexCorner CornerRight(this Direction dir) {
			return HexConstants.directionCorners[dir][1];
		}

		public static Dictionary<SidePoint, Vector3> Points(this Direction dir) {
			return HexConstants.hexSidePoints[dir];
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

	public static class HexCornerExtensions {
		public static Vector3 OuterPosition(this HexCorner corner) {
			return HexConstants.hexCorners[(int) corner];
		}

		public static Vector3 InnerPosition(this HexCorner corner) {
			return HexConstants.hexInnerCorners[(int) corner];
		}

		public static Dictionary<CornerPoint, Vector3> Points(this HexCorner corner) {
			return HexConstants.hexCornerPoints[corner];
		}
	}

	public enum CornerPoint {
		A,
		B,
		C,

		E,
		F,
		G,
	}

	public enum SidePoint {
		C1, C2, C3,
		D1, D2,
		L1, R1,
		E1, E2, E3, E4, E5
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
			{ Direction.SE, new HexCorner[] { HexCorner.E, HexCorner.SE } },
			{ Direction.NE, new HexCorner[] { HexCorner.NE, HexCorner.E } },
			{ Direction.N, new HexCorner[] { HexCorner.NW, HexCorner.NE } },
			{ Direction.NW, new HexCorner[] { HexCorner.W, HexCorner.NW } },
			{ Direction.SW, new HexCorner[] { HexCorner.SW, HexCorner.W } },
			{ Direction.S, new HexCorner[] { HexCorner.SE, HexCorner.SW } },
		};

		public static Dictionary<Direction, Direction> oppositeDirections = new Dictionary<Direction, Direction> {
			{ Direction.SE, Direction.NW },
			{ Direction.NE, Direction.SW },
			{ Direction.N, Direction.S },
			{ Direction.NW, Direction.SE },
			{ Direction.SW, Direction.NE },
			{ Direction.S, Direction.N },
		};

		public static double innerPercent = 0.75;
		public static double edgePercent = 1 - innerPercent;

		public static Vector3[] hexCorners = new Vector3[] {
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.E),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.SE),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.SW),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.W),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.NW),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE, HexCorner.NE),
		};
		

		public static Vector3[] hexInnerCorners = new Vector3[] {
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.E),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.SE),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.SW),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.W),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.NW),
			HexUtils.GetHexCorner(HexConstants.HEX_SIZE * innerPercent, HexCorner.NE),
		};

		public static Dictionary<HexCorner, Dictionary<CornerPoint, Vector3>> hexCornerPoints = new Dictionary<HexCorner, Dictionary<CornerPoint, Vector3>> {
			{ HexCorner.E, HexUtils.GetHexCornerPoints(HexCorner.E) },
			{ HexCorner.NE, HexUtils.GetHexCornerPoints(HexCorner.NE) },
			{ HexCorner.NW, HexUtils.GetHexCornerPoints(HexCorner.NW) },
			{ HexCorner.W, HexUtils.GetHexCornerPoints(HexCorner.W) },
			{ HexCorner.SW, HexUtils.GetHexCornerPoints(HexCorner.SW) },
			{ HexCorner.SE, HexUtils.GetHexCornerPoints(HexCorner.SE) },
		};

		public static Dictionary<Direction, Dictionary<SidePoint, Vector3>> hexSidePoints = new Dictionary<Direction, Dictionary<SidePoint, Vector3>> {
			{ Direction.SE, HexUtils.GetHexSidePoints(Direction.SE) },
			{ Direction.NE, HexUtils.GetHexSidePoints(Direction.NE) },
			{ Direction.N, HexUtils.GetHexSidePoints(Direction.N) },
			{ Direction.NW, HexUtils.GetHexSidePoints(Direction.NW) },
			{ Direction.SW, HexUtils.GetHexSidePoints(Direction.SW) },
			{ Direction.S, HexUtils.GetHexSidePoints(Direction.S) },
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

		public static Vector3 GetHexCorner(double size, HexCorner corner) {
			double deg = 60f * (int) corner;
			double rad = (Math.PI / 180f) * deg;
			return new Vector3((float) (size * Math.Cos(rad)), 0, (float) (size * Math.Sin(rad)));
		}

		public static Dictionary<CornerPoint, Vector3> GetHexCornerPoints(HexCorner corner) {
			var results = new Dictionary<CornerPoint, Vector3>();
			var edge = HexConstants.hexInnerCorners[(int) corner];
			var center = new Vector3(0, 0, 0);
			var B = edge.LinearInterpolate(center, 0.5f);
			var A = edge.LinearInterpolate(B, 0.5f);
			var C = B.LinearInterpolate(center, 0.5f);
			var E = A.LinearInterpolate(B, 0.5f);
			var F = C.LinearInterpolate(center, 0.5f);
			var G = F.LinearInterpolate(center, 0.5f);
			results.Add(CornerPoint.B, B);
			results.Add(CornerPoint.A, A);
			results.Add(CornerPoint.C, C);
			results.Add(CornerPoint.E, E);
			results.Add(CornerPoint.F, F);
			results.Add(CornerPoint.G, G);
			return results;
		}

		public static Dictionary<SidePoint, Vector3> GetHexSidePoints(Direction dir) {
			var results = new Dictionary<SidePoint, Vector3>();
			var c1 = dir.CornerLeft();
			var c2 = dir.CornerRight();
			var c1_point = HexConstants.hexInnerCorners[(int) c1];
			var c2_point = HexConstants.hexInnerCorners[(int) c2];
			var c1_points = GetHexCornerPoints(c1);
			var c2_points = GetHexCornerPoints(c2);
			var center = new Vector3(0, 0, 0);
			var E1 = c1_point.LinearInterpolate(c2_point, 0.5f);
			var C2 = E1.LinearInterpolate(center, 0.5f);
			var C1 = C2.LinearInterpolate(E1, 0.5f);
			var C3 = C2.LinearInterpolate(center, 0.5f);
			var L1 = c1_points[CornerPoint.C].LinearInterpolate(C2, 0.5f);
			var R1 = c2_points[CornerPoint.C].LinearInterpolate(C2, 0.5f);
			var E2 = c1_point.LinearInterpolate(E1, 0.5f);
			var E3 = c2_point.LinearInterpolate(E1, 0.5f);
			var E4 = E1.LinearInterpolate(E2, 0.5f);
			var E5 = E1.LinearInterpolate(E3, 0.5f);
			var D1 = E2.LinearInterpolate(c1_points[CornerPoint.B], 0.5f);
			var D2 = E3.LinearInterpolate(c2_points[CornerPoint.B], 0.5f);
			results.Add(SidePoint.C1, C1);
			results.Add(SidePoint.C2, C2);
			results.Add(SidePoint.C3, C3);
			results.Add(SidePoint.L1, L1);
			results.Add(SidePoint.R1, R1);
			results.Add(SidePoint.E1, E1);
			results.Add(SidePoint.E2, E2);
			results.Add(SidePoint.E3, E3);
			results.Add(SidePoint.E4, E4);
			results.Add(SidePoint.E5, E5);
			results.Add(SidePoint.D1, D1);
			results.Add(SidePoint.D2, D2);

			return results;
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
