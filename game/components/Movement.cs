using System;
using System.Collections.Generic;

public class Movement {
	public Nullable<Hex.OffsetCoord> destination = null;
	public List<Hex.OffsetCoord> movementQueue = new List<Hex.OffsetCoord>();
}
