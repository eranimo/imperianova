using Godot;
using System;
using GameWorld;

/*
Makes World data available globally and in GDScript code
*/
public class WorldData : Node {
    public Vector2 CHUNK_SIZE = new Vector2(50, 50);

    public GameWorld.World world;
    public Vector2 mapSize; 
    public int sealevel;

    public override void _Ready() {
        
    }

    public void AttachWorld(GameWorld.World world) {
        GD.PrintS("Attaching world");
        this.world = world;
        this.mapSize = new Vector2(world.TileWidth, world.TileHeight);
        this.sealevel = world.options.Sealevel;
    }

    public Godot.Collections.Dictionary GetTile(int x, int y) {
        var tile = this.world.GetTile(new Hex.OffsetCoord(x, y));
        var data = new Godot.Collections.Dictionary();
        data.Add("terrain_type", tile.terrainType);
        data.Add("height", tile.height);
        data.Add("temperature", tile.temperature);
        data.Add("rainfall", tile.rainfall);
        return data;
    }
}
