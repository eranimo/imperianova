using Godot;
using DefaultEcs.System;
using System.Collections.Generic;
using DefaultEcs;

public class GameManager {
	GameController gameController;
	public DefaultEcs.World entityManager;

	ISystem<GameDate> daySystems;
	ISystem<GameDate> monthSystems;
	List<ISystem<GameDate>> uiSystems = new List<ISystem<GameDate>>();
	List<ISystem<GameDate>> initSystems = new List<ISystem<GameDate>>();

	public GameManager(GameController gameController) {
		this.gameController = gameController;
		var watch = System.Diagnostics.Stopwatch.StartNew();
		entityManager = new DefaultEcs.World();

		var rand = new RandomNumberGenerator();
		for (int i = 0; i < 2; i++) {
			var popEntity = entityManager.CreateEntity();
			popEntity.Set<TilePosition>(new TilePosition(new Hex.OffsetCoord(rand.RandiRange(0, 100), rand.RandiRange(0, 100))));
			popEntity.Set<PopData>(new PopData(1000, 0.1f));
		}

		var climateSystems = new ClimateSystems(this);

		// for (int i = 0; i < 10; i++) {
		// 	var unitEntity = entityManager.CreateEntity();
		// 	unitEntity.Set<TilePosition>(new TilePosition(new Hex.OffsetCoord(rand.RandiRange(0, 100), rand.RandiRange(0, 100))));

		// 	List<Entity> pops = new List<Entity>();
		// 	for (int u = 0; u < 5; u++) {
		// 		var popEntity = entityManager.CreateEntity();
		// 		popEntity.Set<TilePosition>(new TilePosition(new Hex.OffsetCoord(rand.RandiRange(0, 100), rand.RandiRange(0, 100))));
		// 		popEntity.Set<PopData>(new PopData(1000, 0.01f));
		// 		pops.Add(popEntity);
		// 	}
		// 	var unitData = new UnitData(pops.ToArray(), GameData.UnitType.Warrior);
		// 	unitEntity.Set<UnitData>(unitData);
		// 	var movement = new Movement();
		// 	movement.destination = new Hex.OffsetCoord(rand.RandiRange(0, 100), rand.RandiRange(0, 100));
		// 	unitEntity.Set<Movement>(movement);
		// }

		daySystems = new SequentialSystem<GameDate>(
			new MovementSystem(this.entityManager)
			
		);
		monthSystems = new SequentialSystem<GameDate>(
			new PopGrowthSystem(this.entityManager),
			climateSystems.climateTick
		);

		GD.PrintS($"GameLoop init: {watch.ElapsedMilliseconds}ms");
	}

	public void UpdateDay() {
		daySystems.Update(this.gameController.date.Value);

		foreach(ISystem<GameDate> system in uiSystems) {
			system.Update(this.gameController.date.Value);
		}
	}

	public void UpdateMonth() {
		monthSystems.Update(this.gameController.date.Value);
	}

	public void RegisterViewSystem(ISystem<GameDate> system) {
		uiSystems.Add(system);
	}

	public void UnregisterViewSystem(ISystem<GameDate> system) {
		uiSystems.Remove(system);
	}

	public void RegisterInitSystem(ISystem<GameDate> system) {
		initSystems.Add(system);
	}

	public void UnregisterInitSystem(ISystem<GameDate> system) {
		initSystems.Remove(system);
	}

	public void CallInitSystem(ISystem<GameDate> system) {
		system.Update(this.gameController.date.Value);
	}

}
