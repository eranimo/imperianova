using Godot;
using System;
using LibNoise;
using System.Collections.Generic;

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

	public class WorldOptions {
		public WorldSize Size = WorldSize.Small;
		public int Seed = 12345;
		public int Sealevel = 140;
	}

	public enum WorldSize {
		Small = 0,
		Medium = 1,
		Large = 2,
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
				Size = WorldSize.Small,
				Seed = 12345,
				Sealevel = 140,
			};
		}

		public void Generate() {
			this.InitializeTiles();
			this.GenerateWorld();
		}

		private int GetWorldSize(WorldSize size) {
			switch (size) {
				case WorldSize.Small: return 150;
				case WorldSize.Medium: return 300;
				case WorldSize.Large: return 600;
				default: throw new Exception("Unknown size");
			}
		}

		private void InitializeTiles() {
			var size = GetWorldSize(this.options.Size);
			this.world.TileWidth = size * 2;
			this.world.TileHeight = size;
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
