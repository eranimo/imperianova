using Godot;
using System;

public class WorldView : Spatial {
	public override void _Ready() {
		var chunks = GetNode("Chunks");

		MapChunk chunk = new MapChunk(10, 10);
		chunks.AddChild(chunk);
	}
}
