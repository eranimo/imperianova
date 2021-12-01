using Godot;
using GameWorld;
using System.Collections.Generic;
using DefaultEcs;

public static class MapModes {
	public enum MapMode {
		Terrain = 0,
		Temperature = 1,
		Rainfall = 2,
	}

	public interface IMapMode {
		Color GetTileColor(Entity tile);
	}

	class TerrainMapMode : IMapMode {
		public Color GetTileColor(Entity tile) {
			var tileData = tile.Get<TileData>();
			return TileConstants.TerrainColors[tileData.terrainType];
		}
	}

	class TemperatureMapMode : IMapMode {
		Gradient _gradient;
		public TemperatureMapMode() {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-temperature.tres") as Gradient;
		}
		public Color GetTileColor(Entity tile) {
			var tileData = tile.Get<TileData>();
			return _gradient.Interpolate((float) decimal.Round((decimal) tileData.temperature, 2));
		}
	}

	class RainfallMapMode : IMapMode {
		Gradient _gradient;
		public RainfallMapMode() {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-rainfall.tres") as Gradient;
		}
		public Color GetTileColor(Entity tile) {
			var tileData = tile.Get<TileData>();
			return _gradient.Interpolate((float) decimal.Round((decimal) tileData.temperature, 2));
		}
	}

	public static Dictionary<MapMode, string> mapModeTitles = new Dictionary<MapMode, string> () {
		{ MapMode.Terrain, "Terrain" },
		{ MapMode.Temperature, "Temperature" },
		{ MapMode.Rainfall, "Rainfall" },
	};

	public static Dictionary<MapMode, IMapMode> mapModes = new Dictionary<MapMode, IMapMode> () {
		{ MapMode.Terrain, new TerrainMapMode() },
		{ MapMode.Temperature, new TemperatureMapMode() },
		{ MapMode.Rainfall, new RainfallMapMode() },
	};
}