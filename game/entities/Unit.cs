using Godot;
using Newtonsoft.Json;
using System;

public class Unit : Entity {
	public Unit(Vector2 initial_position) {
		position = new Value<Vector2>(initial_position);
	}

	public class UnitCreated : Message {
		public Unit Unit { set; get; }
	}

	public class UnitDeleted : Message {
		public Unit Unit { set; get; }
	}

	public Value<Vector2> position { get; set; }
	public Tile tile { get; set; }
}
