using Godot;
using System;

public class Interface : Node {
    protected GameState gameState;

	public override void _Ready() {
		gameState = ((Global) GetNode("/root/Global")).gameState;
		GD.Print($"[GameState] Interface ({this.GetType().Name}): init");
        gameState.AddInterface(this);
	}

    public override void _ExitTree() {
        base._ExitTree();
        gameState.RemoveInterface(this);
    }

    virtual public void Init() {}
}
