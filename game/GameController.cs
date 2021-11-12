using Godot;
using System;
using System.Reactive.Subjects;
using GameWorld;
using DefaultEcs;
using DefaultEcs.Threading;
using DefaultEcs.System;

public enum GameSpeed {
	Slow,
	Normal,
	Fast,
}

public class GameController : Node {
	public GameWorld.World world;
	public readonly int TICKS_PER_DAY = 4;

	public BehaviorSubject<GameDate> date = new BehaviorSubject<GameDate>(new GameDate(0));
	public BehaviorSubject<bool> playState = new BehaviorSubject<bool>(false);
	public BehaviorSubject<GameSpeed> speed = new BehaviorSubject<GameSpeed>(GameSpeed.Normal);

	private WorldRenderer worldRenderer;
	private int ticksInDay = 0;
	public GameLoop gameLoop;

	[Signal]
	public delegate void GameInit();

	public override void _Ready() {
		this.ticksInDay = 0;
		this.worldRenderer = (WorldRenderer) GetNode("MapViewport/Viewport/WorldRenderer");
	}

	public override void _ExitTree() {
		// TODO: implement exit
	}

	public override void _Process(float delta) {
		if (!this.Playing) {
			return;
		}

		if (this.ticksInDay == 0) {
			int ticksLeft = this.SpeedTicks;
			this.ProcessDay();
			this.ticksInDay = ticksLeft;
		} else {
			this.ticksInDay--;
		}
	}

	private void ProcessDay() {
		date.OnNext(date.Value.NextDay());

		gameLoop.UpdateDay();

		if (date.Value.isFirstOfMonth) {
			gameLoop.UpdateMonth();
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_playstate")) {
			this.playState.OnNext(!this.playState.Value);
			GetTree().SetInputAsHandled();
		}
	}

	public bool Playing {
		get {
			return this.playState.Value;
		}
		set {
			this.playState.OnNext(value);
		}
	}

	public void ToggleSpeed() {
		switch (this.speed.Value) {
			case GameSpeed.Slow: 
				this.speed.OnNext(GameSpeed.Normal);
				return;
			case GameSpeed.Normal:
				this.speed.OnNext(GameSpeed.Fast);
				return;
			case GameSpeed.Fast:
				this.speed.OnNext(GameSpeed.Slow);
				return;
		}
	}

	public void TogglePlay() {
		if (this.Playing) {
			this.Pause();
		} else {
			this.Play();
		}
	}

	public void NewGame() {
		var world = GameWorld.World.Generate();
		this.Init(world);
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

	private void Init(GameWorld.World world) {
		this.world = world;
		this.date.OnNext(new GameDate(0));
		this.Render();
		this.gameLoop = new GameLoop(this, world);
		EmitSignal(nameof(GameInit));
	}

	private void Render() {
		WorldData worldData = GetNode<WorldData>("/root/WorldData");
		worldData.AttachWorld(this.world);
		this.worldRenderer.RenderWorld(this.world);
	}

	private int SpeedTicks {
		get {
			switch (this.speed.Value) {
				case GameSpeed.Slow: return 4 * this.TICKS_PER_DAY;
				case GameSpeed.Normal: return 2 * this.TICKS_PER_DAY;
				case GameSpeed.Fast: return 1 * this.TICKS_PER_DAY;
				default: throw new Exception("Unknown Speed");
			}
		}
	}
}
