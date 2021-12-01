using Godot;
using System;
using System.Reactive.Subjects;

public class InputManager : Node {
	public BehaviorSubject<Nullable<Hex.OffsetCoord>> SelectedTile = new BehaviorSubject<Nullable<Hex.OffsetCoord>>(null);
    public BehaviorSubject<MapModes.MapMode> ActiveMapMode = new BehaviorSubject<MapModes.MapMode>(MapModes.MapMode.Terrain);

	public float zoom;

	[Signal]
	public delegate void CameraZoom(float zoom);

	[Signal]
	public delegate void CameraMove(Vector2 offset);

	[Signal]
	public delegate void TileHovered(Vector2 tile);

	[Signal]
	public delegate void TileSelected(Vector2 tile);
}
