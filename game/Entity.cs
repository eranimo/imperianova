using Godot;
using System;

/// <summary>Game entity</summary>
public class Entity {
    /// <summary>Create a game entity</summary>
    public Entity(String id_ = null) {
		if (id_ == null) {
			id = new Guid().ToString();
		} else {
			id = id_;
		}
	}

	public string id;
	public bool isDirty = false;
	internal GameState gameState;

	public void Attach(GameState _gameState) {
		gameState = _gameState;
	}
}
