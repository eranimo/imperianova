using Godot;
using System;
using Entities;

public class UnitManager : Manager {
    public override void Init() {
        Unit unit = new Unit(new Vector2(0, 0));
		gameState.AddEntity(unit);
    }
}
