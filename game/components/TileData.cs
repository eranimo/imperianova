using DefaultEcs;
using GameWorld;
using Hex;

public struct AirLayer {
	public double pressure;
	public double temperature;
	public double humidity;
	public double clouds;
}

public struct TileData {
	public double latitude;
	public double longitude;

	public TerrainType terrainType;
	public float height;
	public bool is_ocean;

	// OLD
	public float temperature;
	public float rainfall;

	// climate simulation
	// these values change every climate tick
	public double insolation;
	public AirLayer[] air_layers;
	public double surface_temperature;
	public double precipitation;
}
