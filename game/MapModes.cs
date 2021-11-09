using Godot;
using GameWorld;
using System.Collections.Generic;

public static class MapModes {
	public enum MapMode {
		Terrain = 0,
		Temperature = 1,
		Rainfall = 2,
	}

	public interface IMapMode {
		Color GetTileColor(Tile tile);
	}

	class TerrainMapMode : IMapMode {
		public Color GetTileColor(Tile tile) {
			return TileConstants.TerrainColors[tile.terrainType];
		}
	}

	class TemperatureMapMode : IMapMode {
		Gradient _gradient;
		public TemperatureMapMode() {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-temperature.tres") as Gradient;
		}
		public Color GetTileColor(Tile tile) {
			return _gradient.Interpolate((float) decimal.Round((decimal) tile.temperature, 2));
		}
	}

	class RainfallMapMode : IMapMode {
		Gradient _gradient;
		public RainfallMapMode() {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-rainfall.tres") as Gradient;
		}
		public Color GetTileColor(Tile tile) {
			return _gradient.Interpolate((float) decimal.Round((decimal) tile.temperature, 2));
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