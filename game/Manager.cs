using Godot;
using System;

public class Manager : Node {
	protected GameState gameState;

	public override void _Ready() {
		gameState = ((Global) GetNode("/root/Global")).gameState;
		GD.Print($"[GameState] Manager ({this.GetType().Name}): init");

		Init();
	}

	virtual public void Init() {}
	virtual public void Update() {}
}
