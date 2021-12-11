using Godot;
using GameWorld;
using System.Collections.Generic;
using DefaultEcs;

public static class MapModes {
	public enum MapMode {
		Terrain = 0,
		Temperature = 1,
		Rainfall = 2,
		Population = 3,
		Insolation = 4,
	}

	public static Dictionary<MapMode, string> mapModeTitles = new Dictionary<MapMode, string> () {
		{ MapMode.Terrain, "Terrain" },
		{ MapMode.Temperature, "Temperature" },
		{ MapMode.Rainfall, "Rainfall" },
		{ MapMode.Population, "Population" },
		{ MapMode.Insolation, "Insolation" },
	};
}