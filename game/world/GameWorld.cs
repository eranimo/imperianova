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

		public static OffsetCoord oddq_offset_neighbor(OffsetCoord hex, Direction direction) {
			var parity = hex.Col & 1;
			var dir = HexConstants.oddq_directions[parity, (int) direction];
			return new OffsetCoord(hex.Col + dir.Col, hex.Row + dir.Row);
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
			return new Vector2((int) x, (int) y);
		}

		public static AxialCoord HexToPixel(AxialCoord hex) {
			var x = HexConstants.HEX_SIZE * (3.0/2 * hex.q);
			var y = HexConstants.HEX_SIZE * (Math.Sqrt(3) / 2 * hex.q + Math.Sqrt(3) * hex.r);
			return new AxialCoord(x, y);
		}
	}
}

namespace GameWorld {
	public enum TerrainType {
		Ocean,
		Grassland,
		Desert,
		Forest,
	}

	public static class TileConstants {
		public static Dictionary<TerrainType, Color> TerrainColors = new Dictionary<TerrainType, Color> () {
			{ TerrainType.Ocean, new Color("#ff1f538c") },
			{ TerrainType.Grassland, new Color("#ff529a3b") },
			{ TerrainType.Desert, new Color("#ffcec27e") },
			{ TerrainType.Forest, new Color("#ff25562e") }
		};
	}

	public class Tile {
		private World world;
		public Hex.OffsetCoord position;

		public TerrainType terrainType;
		public float height;
		public float temperature;
		public float rainfall;

		public Tile(World world, Hex.OffsetCoord position) {
			this.world = world;
			this.position = position;
		}

		public Tile GetNeighbor(Hex.Direction direction) {
			return this.world.GetTile(Hex.HexUtils.oddq_offset_neighbor(this.position, direction));
		}
	}

	public struct WorldOptions {
		public int Size;
		public int Seed;
		public int Sealevel;
	}

	class WorldNoise {
		public int width;
		public int height;
		public int octaves;
		public int frequency;
		public float amplitude;
		private LibNoise.Primitive.ImprovedPerlin noise;

		public WorldNoise(int width, int height, int seed, int octaves = 5, int frequency = 2, float amplitude = 0.5f) {
			this.width = width;
			this.height = height;
			this.octaves = octaves;
			this.frequency = frequency;
			this.amplitude = amplitude;
			this.noise = new LibNoise.Primitive.SimplexPerlin(seed, NoiseQuality.Best);
		}

		/// <summary>Gets a coordinate noise value projected onto a sphere</summary>
		public float Get(int x, int y) {
			var coordLong = ((x / (double) this.width) * 360) - 180;
			var coordLat = ((-y / (double) this.height) * 180) + 90;
			var inc = ((coordLat + 90.0) / 180.0) * Math.PI;
			var azi = ((coordLong + 180.0) / 360.0) * (2 * Math.PI);
			var nx = (float) (Math.Sin(inc) * Math.Cos(azi));
			var ny = (float) (Math.Sin(inc) * Math.Sin(azi));
			var nz = (float) (Math.Cos(inc));

			float amplitude = 1;
			float freq = 1;
			var v = 0f;
			for (var i = 0; i < this.octaves; i++) {
				v += this.noise.GetValue(nx * freq, ny * freq, nz * freq) * amplitude;
				amplitude *= this.amplitude;
				freq *= this.frequency;
			}

			v = (v + 1) / 2;
			return v * 255;
		}
	}

	/*
	Generates a World
	*/
	public class WorldGenerator {
		public WorldOptions options;
		private World world;

		public WorldGenerator(World world) {
			this.world = world;
			this.options = new WorldOptions {
				Size = 150,
				Seed = 12345,
				Sealevel = 140,
			};
		}

		public void Generate() {
			this.InitializeTiles();
			this.GenerateWorld();
		}

		private void InitializeTiles() {
			this.world.TileWidth = this.options.Size * 2;
			this.world.TileHeight = this.options.Size;
			this.world.Tiles = new Tile[this.world.TileWidth, this.world.TileHeight];

			for (var x = 0; x < this.world.TileWidth; x++) {
				for (var y = 0; y < this.world.TileHeight; y++) {
					this.world.Tiles[x, y] = new Tile(this.world, new Hex.OffsetCoord(x, y));
				}
			}
		}

		private void GenerateWorld() {
			var heightNoise = new WorldNoise(this.world.TileWidth, this.world.TileHeight, this.options.Seed);
			var temperatureNoise = new WorldNoise(this.world.TileWidth, this.world.TileHeight, this.options.Seed * 2);
			var rainfallNoise = new WorldNoise(this.world.TileWidth, this.world.TileHeight, this.options.Seed * 3);

			for (var x = 0; x < this.world.TileWidth; x++) {
				for (var y = 0; y < this.world.TileHeight; y++) {
					this.world.Tiles[x, y].height = heightNoise.Get(x, y);
					this.world.Tiles[x, y].temperature = temperatureNoise.Get(x, y) / 255;
					this.world.Tiles[x, y].rainfall = rainfallNoise.Get(x, y) / 255;
				}
			}

			foreach (Tile tile in this.world.Tiles) {
				if (tile.height < this.options.Sealevel) {
					tile.terrainType = TerrainType.Ocean;
				} else {
					if (tile.temperature < 0.60) {
						if (tile.rainfall < 0.5) {
							tile.terrainType = TerrainType.Grassland;
						} else {
							tile.terrainType = TerrainType.Forest;
						}
					} else {
						tile.terrainType = TerrainType.Desert;
					}
				}
			}
		}
	}

	/*
	Container for Tiles
	*/
	public class World {
		public int TileWidth;
		public int TileHeight;
		public WorldOptions options;
		public Tile[,] Tiles;

		public World() {

		}

		public Tile GetTile(Hex.OffsetCoord coord) {
			return this.Tiles[coord.Col, coord.Row];
		}

		public bool IsValidTile(Hex.OffsetCoord coord) {
			return coord.Col >= 0 && coord.Row >= 0 && coord.Col < TileWidth && coord.Row < TileHeight;
		}

		public static World Generate() {
			World world = new World();
			WorldGenerator worldGenerator = new WorldGenerator(world);
			worldGenerator.Generate();
			world.options = worldGenerator.options; 
			return world;
		}

		public static World Load() {
			World world = new World();
			// TODO: implement
			return world;
		}
	}
}
