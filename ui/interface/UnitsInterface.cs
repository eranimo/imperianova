using Godot;
using System;
using System.Collections.Generic;

/*
- on init, instantiate a unit node for each unit entity
- on UnitAdded message, add unit node
- on UnitMoved message, update unit node position
- on UnitDeleted message, delete unit node
*/
public class UnitsInterface : Interface {
    Query units;
    public override void Init() {
        units = gameState.Query().WithType("Unit").Done();
    }
}