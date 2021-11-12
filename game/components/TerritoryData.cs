using System.Collections.Generic;
using DefaultEcs;
using GameWorld;
using Hex;

public struct TerritoryData {
	public HashSet<Hex.OffsetCoord> tiles;

    public TerritoryData(OffsetCoord[] tiles) {
        this.tiles = new HashSet<OffsetCoord>(tiles);
    }
}
