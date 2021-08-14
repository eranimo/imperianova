using Godot;
using System;
using System.Collections.Generic;

public class UnitsInterface : Interface {
	Query units;

	public override void Init() {
		units = gameState.Query().WithType("Unit").Done();
		units.EntityAdded += OnEntityAdded;
		units.EntityRemoved += OnEntityRemoved;
	}

	public void OnEntityAdded(object sender, Entity.EntityEventArgs e) {
		var unit = (Unit) e.entity;
		unit.position.ValueChanged += OnUnitPositionChanged;
	}

	public void OnEntityRemoved(object sender, Entity.EntityEventArgs e) {
		var unit = (Unit) e.entity;
		unit.position.ValueChanged -= OnUnitPositionChanged;
	}

	public void OnUnitPositionChanged(object sender, Entity.Value<Tile>.ValueChangedEventArgs e) {
		// GD.PrintS("OLD: ", e.oldValue, "NEW: ", e.newValue);
	}
}
