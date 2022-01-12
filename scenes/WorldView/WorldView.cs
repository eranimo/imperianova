using Godot;
using System;
using Hex;
using System.Reactive.Subjects;

public enum MapEditorTool {
	Terrain,
	Rivers,
}

public class WorldViewSettings {
	public BehaviorSubject<bool> showGrid = new BehaviorSubject<bool>(true);
}

public class WorldView : Spatial {
	private HexGrid grid;
	private ChunksContainer chunksContainer;
	private bool isDragging;

	// map editor
	MapEditorTool Tool = MapEditorTool.Terrain;
	int TerrainToolHeight = 10;
	HexCell riverToolLastCell = null;

	public override void _Ready() {
		this.chunksContainer = (ChunksContainer) GetNode("ChunksContainer");
		var watch = System.Diagnostics.Stopwatch.StartNew();

		// create HexGrid
		this.grid = new HexGrid(new OffsetCoord(100, 100));
		var heightNoise = new GameWorld.WorldNoise(grid.Size.Col, grid.Size.Row, 123);
		for (var x = 0; x < grid.Size.Col; x++) {
			for (var y = 0; y < grid.Size.Row; y++) {
				var pos = new OffsetCoord(x, y);
				var cell = new HexCell(pos);
				cell.Height = Math.Round((heightNoise.Get(x, y) / 255f) * 20) * 5;
				cell.WaterLevel = 47.5;
				decideCellTerrainType(cell);
				grid.AddCell(cell);
			}
		}

		chunksContainer.SetupChunks(grid);
		GD.PrintS($"WorldView init: {watch.ElapsedMilliseconds}ms");

		chunksContainer.pressedCell.Subscribe((HexCell cell) => {
			if (cell == null) {
				return;
			}
			GD.PrintS("Pressed on cell:", cell.Position);
			// GD.PrintT(new string[] {
			// 	$"SE: {cell.GetNeighbor(Hex.Direction.SE)?.Height}",
			// 	$"NE: {cell.GetNeighbor(Hex.Direction.NE)?.Height}",
			// 	$"N: {cell.GetNeighbor(Hex.Direction.N)?.Height}",
			// 	$"NW: {cell.GetNeighbor(Hex.Direction.NW)?.Height}",
			// 	$"SW: {cell.GetNeighbor(Hex.Direction.SW)?.Height}",
			// 	$"S: {cell.GetNeighbor(Hex.Direction.S)?.Height}",
			// });
			if (Tool == MapEditorTool.Terrain) {
				cell.Height = TerrainToolHeight;
				decideCellTerrainType(cell);
				chunksContainer.RegenerateCell(cell);
			} else if (Tool == MapEditorTool.Rivers) {

			}
		});

		chunksContainer.hoveredCell.Subscribe((HexCell cell) => {
			if (cell == null) {
				return;
			}
			if (Tool == MapEditorTool.Rivers && isDragging) {
				if (riverToolLastCell != null && riverToolLastCell != cell) {
					Direction? dir = riverToolLastCell.GetDirectionOfNeighbor(cell);
					if (!(dir is null)) {
						GD.PrintS($"Adding river from {riverToolLastCell.Position} to {cell.Position}");
						var oppositeDir = HexConstants.oppositeDirections[(Direction) dir];
						// remove old
						// riverToolLastCell.OutgoingRivers.Remove(oppositeDir);
						// cell.IncomingRivers.Remove((Direction) dir);

						// add new
						riverToolLastCell.OutgoingRivers.Add((Direction) dir);
						cell.IncomingRivers.Add(oppositeDir);

						chunksContainer.RegenerateCell(cell);
					}
				}
				riverToolLastCell = cell;
			}
		});

		(FindNode("TerrainHeight") as SpinBox).Value = TerrainToolHeight;
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_select")) {
			isDragging = true;
		} else if (@event.IsActionReleased("ui_select")) {
			isDragging = false;
			riverToolLastCell = null;
		}
	}

	private void decideCellTerrainType(HexCell cell) {
		if (cell.Height > 75) {
			cell.Color = new Color("#619960");
		} else if (cell.Height > 50) {
			cell.Color = new Color("#208a0e");
		} else if (cell.Height > 48) {
			cell.Color = new Color("#ebe571");
		} else if (cell.Height > 15) {
			cell.Color = new Color("#0356fc");
		} else {
			cell.Color = new Color("#003bb0");
		}
	}

	private void _on_GridToggle_toggled(bool button_pressed) {
		grid.viewSettings.showGrid.OnNext(button_pressed);
	}

	private void _on_TerrainHeight_value_changed(float value) {
		TerrainToolHeight = (int) value;
	}

	private void _on_EditorSettingsToggle_toggled(bool button_pressed) {
		var container = (FindNode("SettingsContainer") as Control);
		container.Visible = !container.Visible;
	}

	private void _on_TerrainToolCheckbox_toggled(bool button_pressed) {
		if (button_pressed) {
			Tool = MapEditorTool.Terrain;
		}
	}

	private void _on_RiverToolCheckbox_toggled(bool button_pressed) {
		if (button_pressed) {
			Tool = MapEditorTool.Rivers;
		}
	}
}
