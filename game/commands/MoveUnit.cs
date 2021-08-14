using Godot;
using System;

public class MoveUnit : Command<Unit> {
	public MoveUnit(Unit target_, Vector2 moveLocation_) : base(target_) {
		moveLocation = moveLocation_;
	}

	public Vector2 moveLocation;

	public override void Execute() {
		// target.position.Set();
	}
}
