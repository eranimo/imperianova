using Godot;
using DefaultEcs.System;

class GameLoop {
	GameController gameController;
	DefaultEcs.World entityManager;

	ISystem<GameDate> daySystems;
	ISystem<GameDate> monthSystems;

	public GameLoop(GameController gameController, GameWorld.World world) {
		this.gameController = gameController;
		var watch = System.Diagnostics.Stopwatch.StartNew();
		entityManager = new DefaultEcs.World();

		// foreach (Tile tile in world.Tiles) {
		// 	var tileEntity = entityManager.CreateEntity();
		// 	tileEntity.Set<TilePosition>(new TilePosition(tile.position));
		// }

		var rand = new RandomNumberGenerator();
		for (int i = 0; i < 2; i++) {
			var popEntity = entityManager.CreateEntity();
			popEntity.Set<TilePosition>(new TilePosition(new Hex.OffsetCoord(rand.RandiRange(0, 100), rand.RandiRange(0, 100))));
			popEntity.Set<PopData>(new PopData(1000, 0.01f));
		}

		daySystems = new SequentialSystem<GameDate>();
		monthSystems = new SequentialSystem<GameDate>(
			new PopGrowthSystem(this.entityManager)
		);

		GD.PrintS($"GameLoop init: {watch.ElapsedMilliseconds}ms");
	}

	public void UpdateDay() {
		daySystems.Update(this.gameController.date.Value);
	}

	public void UpdateMonth() {
		monthSystems.Update(this.gameController.date.Value);
	}
}
