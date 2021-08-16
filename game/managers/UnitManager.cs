using Godot;
using System;

public class UnitManager : Manager {
	public override void Init() {
		var tile = new Tile(0, 0);
		var tile2 = new Tile(0, 1);
		gameState.AddEntity(tile);
		gameState.AddEntity(tile2);
		var tile_id = tile.id;
		GD.Print("[UnitManager] init");

		Unit unit = new Unit(new Vector2(0, 0));
		unit.tile = tile;
		gameState.AddEntity(unit);

		Unit unit2 = new Unit(new Vector2(0, 1));
		unit2.tile = tile2;
		gameState.AddEntity(unit2);

		GD.Print("> Export");
		gameState.debug();
		var exported = gameState.export();
		GD.Print(exported)
		;
		GD.Print("> Clear");
		gameState.clear();
		gameState.debug();

		GD.Print("> Import");
		gameState.import(exported);
		gameState.debug();
	}
}
