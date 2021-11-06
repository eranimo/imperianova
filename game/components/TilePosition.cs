using DefaultEcs;
using GameWorld;
using Hex;

public struct TilePosition {
	public Hex.OffsetCoord position;

    public TilePosition(OffsetCoord position) {
        this.position = position;
    }
}
