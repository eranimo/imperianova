using Godot;
using Newtonsoft.Json;
using System;

public class Unit : Entity {
	public Unit(Tile initial_position) {
		position = new Value<Tile>(initial_position);
	}

	public class UnitCreated : Message {
		public Unit Unit { set; get; }
	}

	public class UnitDeleted : Message {
		public Unit Unit { set; get; }
	}

	public Value<Tile> position = new Value<Tile>();
}
