using Godot;
using System;
using System.Reactive.Subjects;
using GameWorld;

public enum Speed {
	Slow,
	Normal,
	Fast,
}


public class Game : Node {
	public GameWorld.World world;
	public readonly int TICKS_PER_DAY = 4;

	public BehaviorSubject<int> date = new BehaviorSubject<int>(0);
	public BehaviorSubject<bool> playState = new BehaviorSubject<bool>(false);
	public BehaviorSubject<Speed> speed = new BehaviorSubject<Speed>(Speed.Normal);

	private GameState gameState;
	private Godot.Object worldMap;
	private int ticksInDay = 0;

	public override void _Ready() {
		this.ticksInDay = 0;
		this.gameState = GetNode<GameState>("GameState");
		this.worldMap = GetNode("MapViewport/Viewport/WorldMap");
	}

	public override void _ExitTree() {
		// TODO: implement exit
	}

	public override void _Process(float delta) {
		base._Process(delta);
		this.gameState.Process();
		
		if (!this.IsPlaying()) {
			return;
		}

		if (this.ticksInDay == 0) {
			int ticksLeft = this.GetSpeedTicks();
			this.date.OnNext(date.Value + 1);
			this.ticksInDay = ticksLeft;
		} else {
			this.ticksInDay--;
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_playstate")) {
			this.playState.OnNext(!this.playState.Value);
			GetTree().SetInputAsHandled();
		}
	}

	public bool IsPlaying() {
		return this.playState.Value;
	}

	public void SetPlaying(bool playing) {
		this.playState.OnNext(playing);
	}

	public void ToggleSpeed() {
		switch (this.speed.Value) {
			case Speed.Slow: 
				this.speed.OnNext(Speed.Normal);
				return;
			case Speed.Normal:
				this.speed.OnNext(Speed.Fast);
				return;
			case Speed.Fast:
				this.speed.OnNext(Speed.Slow);
				return;
		}
	}

	public void TogglePlay() {
		if (this.IsPlaying()) {
			this.Pause();
		} else {
			this.Play();
		}
	}

	public void NewGame() {
		this.world = GameWorld.World.Generate();
		this.Render();
	}

	public void LoadGame() {
		// TODO: implement
	}

	public void Play() {
		this.playState.OnNext(true);
	}

	public void Pause() {
		this.playState.OnNext(false);
	}

	private void Render() {
		WorldData worldData = GetNode<WorldData>("/root/WorldData");
		worldData.AttachWorld(this.world);
		this.worldMap.Call("render");
	}

	private int GetSpeedTicks() {
		switch (this.speed.Value) {
			case Speed.Slow: return 4 * this.TICKS_PER_DAY;
			case Speed.Normal: return 2 * this.TICKS_PER_DAY;
			case Speed.Fast: return 1 * this.TICKS_PER_DAY;
			default: throw new Exception("Unknown Speed");
		}
	}
}
