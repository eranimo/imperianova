using Godot;
using System;
using LibNoise;
using System.Collections.Generic;
using DefaultEcs;

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

	public class WorldOptions {
		public WorldSize Size = WorldSize.Small;
		public int Seed = 12345;
		public int Sealevel = 140;
		public double AxialTilt = 23.45;
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

	static class Climate {
		private static double DegToRad(double deg) {
			return deg * (Math.PI / 180);
		}
		public static double CalculateSolarInsolation(WorldInfo worldInfo, TileData tileData, int dayOfYear) {
			var tilt = worldInfo.options.AxialTilt;
			var lat = DegToRad(tileData.latitude);
			var rot = (360f) / 24f;
			var solarConstant = 1362;
			double declination = DegToRad(tilt * Math.Sin((360d/365d) * (284d + (double) (dayOfYear + 1))));
			double insolation = 0;
			for (int h = 1; h <= 24; h++) {
				var H = rot * (h - 12);
				var Z = Math.Acos((Math.Sin(lat) * Math.Sin(declination)) + (Math.Cos(lat) * Math.Cos(declination) * Math.Cos(H)));
				insolation += Math.Max(0, (solarConstant * Math.Cos(Z)));
			}
			return insolation / 24f;
		}
	}

	/*
	Generates a World
	*/
	public class WorldGenerator {
		public WorldOptions options;
		private GameManager gm;
		private int TileWidth;
		private int TileHeight;
		private WorldInfo worldInfo;
		private GameDate gameDate;

		public WorldGenerator(GameManager gm, GameDate gameDate) {
			this.gm = gm;
			this.gameDate = gameDate;
			this.options = new WorldOptions();
		}

		public WorldInfo Generate() {
			this.InitializeTiles();
			this.GenerateWorld();
			return this.worldInfo;
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
			this.TileWidth = size * 2;
			this.TileHeight = size;
			worldInfo = new WorldInfo {
				options = options,
				size = new Hex.OffsetCoord(this.TileWidth, this.TileHeight),
			};
		}

		private void GenerateWorld() {
			var heightNoise = new WorldNoise(this.TileWidth, this.TileHeight, this.options.Seed);
			var temperatureNoise = new WorldNoise(this.TileWidth, this.TileHeight, this.options.Seed * 2);
			var rainfallNoise = new WorldNoise(this.TileWidth, this.TileHeight, this.options.Seed * 3);

			var tiles = new List<Entity>();

			for (var x = 0; x < this.TileWidth; x++) {
				for (var y = 0; y < this.TileHeight; y++) {
					var height = heightNoise.Get(x, y);
					var temperature = temperatureNoise.Get(x, y) / 255;
					var rainfall = rainfallNoise.Get(x, y) / 255;
					var tilePosition = new TilePosition(new Hex.OffsetCoord(x, y));
					var coordLong = ((x / (double) this.TileWidth) * 360) - 180;
					var coordLat = ((-y / (double) this.TileHeight) * 180) + 90;
					var tileData = new TileData {
						longitude = coordLong,
						latitude = coordLat,
						height = height,
						temperature = temperature,
						rainfall = rainfall,
					};
					tileData.insolation = Climate.CalculateSolarInsolation(worldInfo, tileData, 0);
					var entity = this.gm.entityManager.CreateEntity();
					entity.Set<TilePosition>(tilePosition);
					entity.Set<TileData>(tileData);
					tiles.Add(entity);
				}
			}

			foreach (Entity tile in tiles) {
				var tileData = tile.Get<TileData>();
				if (tileData.height < this.options.Sealevel) {
					tileData.terrainType = TerrainType.Ocean;
				} else {
					if (tileData.temperature < 0.60) {
						if (tileData.rainfall < 0.5) {
							tileData.terrainType = TerrainType.Grassland;
						} else {
							tileData.terrainType = TerrainType.Forest;
						}
					} else {
						tileData.terrainType = TerrainType.Desert;
					}
				}
				tile.Set<TileData>(tileData);
			}
		}
	}

	/*
	Container for Tiles
	*/
	public class World {
		private WorldInfo worldInfo;
		private Dictionary<TilePosition, TileData> tilePositionMap;
		private Dictionary<Hex.OffsetCoord, TilePosition> tileCoordMap;

		public IEnumerable<Entity> tiles { get; private set; }

		public int TileWidth { get { return this.worldInfo.size.Col; } }
		public int TileHeight { get { return this.worldInfo.size.Row; } }
		public WorldOptions options { get { return this.worldInfo.options; } }

		public World(GameManager gameManager) {
			this.worldInfo = gameManager.entityManager.Get<WorldInfo>();
			this.tilePositionMap = new Dictionary<TilePosition, TileData>();
			this.tileCoordMap = new Dictionary<Hex.OffsetCoord, TilePosition>();

			this.tiles = gameManager.entityManager.GetEntities()
				.With<TilePosition>()
				.With<TileData>()
				.AsEnumerable();

			foreach (Entity tile in tiles) {
				var tp = tile.Get<TilePosition>();
				tilePositionMap.Add(tp, tile.Get<TileData>());
				tileCoordMap.Add(tp.position, tp);
			}
		}

		public TileData GetTile(Hex.OffsetCoord coord) {
			return GetTile(this.tileCoordMap[coord]);
		}

		public TileData GetTile(TilePosition tilePosition) {
			return tilePositionMap[tilePosition];
		}

		public bool IsValidTile(Hex.OffsetCoord coord) {
			return coord.Col >= 0 && coord.Row >= 0 && coord.Col < this.worldInfo.size.Col && coord.Row < this.worldInfo.size.Row;
		}

		public TileData GetNeighbor(Hex.OffsetCoord position, Hex.Direction direction) {
			return GetTile(GetNeighborCoord(position, direction));
		}

		public Hex.OffsetCoord GetNeighborCoord(Hex.OffsetCoord position, Hex.Direction direction) {
			return Hex.HexUtils.GetNeighbor(position, direction);
		}
	}
}
