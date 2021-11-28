using GameWorld;

public static class GameSetup {
	public class GameSettings {
		public WorldOptions WorldOptions = new WorldOptions();
		public int StartingPops;
		public int GameSeed;
	}

	public static void NewGame(GameController game) {
		var world = GameWorld.World.Generate();
		game.Init(world);
	}
}
