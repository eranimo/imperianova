using Godot;
using System;
using System.Reactive.Subjects;

public class InputManager : Node {
	public BehaviorSubject<GameWorld.Tile> SelectedTile = new BehaviorSubject<GameWorld.Tile>(null);
    public BehaviorSubject<MapModes.MapMode> ActiveMapMode = new BehaviorSubject<MapModes.MapMode>(MapModes.MapMode.Terrain);

	[Signal]
	public delegate void CameraZoom(float zoom);

	[Signal]
	public delegate void CameraMove(Vector2 offset);

	[Signal]
	public delegate void TileHovered(Vector2 tile);

	[Signal]
	public delegate void TileSelected(Vector2 tile);
}
