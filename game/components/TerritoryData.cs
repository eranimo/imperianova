using System.Collections.Generic;
using DefaultEcs;
using GameWorld;
using Hex;

public struct TerritoryData {
	public List<Hex.OffsetCoord> tiles;

    public TerritoryData(OffsetCoord position) {
        this.tiles = new List<OffsetCoord>();
    }
}
