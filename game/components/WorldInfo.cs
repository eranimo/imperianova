using Godot;

// global component
public struct WorldInfo {
	public GameWorld.WorldOptions options;
	public Hex.OffsetCoord size;

	public bool IsValidTile(Hex.OffsetCoord coord) {
		return coord.Col >= 0 && coord.Row >= 0 && coord.Col < size.Col && coord.Row < size.Row;
	}
}