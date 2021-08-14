using Godot;
using Newtonsoft.Json;
using System;

public class Tile : Entity {
	public Tile(uint _x, uint _y) {
		x = _x;
		y = _y;
	}

	public uint x { get; set; }
	public uint y { get; set; }

	public override string ToString() {
		return $"Tile({x}, {y})";
	}
}
