using Hex;
using System.Collections.Generic;
using System.Reactive.Subjects;

public class HexGrid {
	public OffsetCoord Size;
	private Dictionary<OffsetCoord, HexCell> cells = new Dictionary<OffsetCoord, HexCell>();

	public WorldViewSettings viewSettings = new WorldViewSettings();

	public HexGrid(OffsetCoord size) {
		Size = size;
	}

	public void AddCell(HexCell cell) {
		cells.Add(cell.Position, cell);
		cell.Grid = this;
	}

	public HexCell GetCell(OffsetCoord pos) {
		try {
			return cells[pos];
		} catch (KeyNotFoundException) {
			return null;
		}
	}
}
