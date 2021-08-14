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
		for(var i = 0; i < 2; i++) {
			Unit unit = new Unit(tile);
			gameState.AddEntity(unit);
			GD.PrintS("Entity", unit.id);

			unit.position.Set(tile2);

			// gameState.RemoveEntity(unit);
		}
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
