using GameWorld;

public static class GameSetup {
	public class GameSettings {
		public WorldOptions WorldOptions = new WorldOptions();
		public int StartingPops;
		public int GameSeed;
	}

	public static void NewGame(GameController game) {
		var worldGen = new GameWorld.WorldGenerator(game.gameManager);
		var worldInfo = worldGen.Generate();
		game.gameManager.entityManager.Set<WorldInfo>(worldInfo);
		game.Init(worldInfo);
	}
}
