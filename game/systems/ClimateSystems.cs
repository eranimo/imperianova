using System;
using System.Diagnostics;
using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using Godot;

class ClimateSystems {
	GameManager gm;
	public ParallelSystem<GameDate> climateTick;

	public ClimateSystems(GameManager gm) {
		this.gm = gm;

		this.climateTick = new ParallelSystem<GameDate>(
			new InsolationSystem(gm),
			new DefaultParallelRunner(System.Environment.ProcessorCount)
		);
	}

	public void Update(GameDate state) {
		climateTick.Update(state);
	}

	[With(typeof(TileData))]
	[With(typeof(TilePosition))]
	class InsolationSystem : AEntitySetSystem<GameDate> {
		private Stopwatch watch;

		public InsolationSystem(GameManager gm) : base(gm.entityManager) {}

		protected override void PreUpdate(GameDate state) {
			this.watch = new System.Diagnostics.Stopwatch();
			this.watch.Start();
			GD.PrintS("Update insolation", state.DayOfYear);
		}

		protected override void Update(GameDate state, in Entity entity) {
			ref TilePosition tilePosition = ref entity.Get<TilePosition>();
			ref TileData tileData = ref entity.Get<TileData>();
			ref WorldInfo worldInfo = ref this.World.Get<WorldInfo>();

			tileData.insolation = GameWorld.Climate.CalculateSolarInsolation(worldInfo, tileData, state.DayOfYear);
			entity.NotifyChanged<TileData>();
		}

		protected override void PostUpdate(GameDate state) {
			this.watch.Start();
			GD.PrintS($"Insolation update took {watch.ElapsedMilliseconds}ms");
		}
	}
}