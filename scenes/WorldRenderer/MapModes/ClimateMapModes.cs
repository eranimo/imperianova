using DefaultEcs;
using DefaultEcs.System;
using Godot;
using System;

namespace MapModeSystems {
	public class InsolationMapMode : MapModeSystem {
		public override MapModes.MapMode mapMode { get { return MapModes.MapMode.Insolation; } }
		private Gradient _gradient;
		public InsolationMapMode(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-temperature.tres") as Gradient;
		}

		protected override void ProcessEntity(Entity entity) {
			var tileData = entity.Get<TileData>();
			var tilePos = entity.Get<TilePosition>();
			Color color = _gradient.Interpolate((float) Math.Max(Math.Min(tileData.insolation, 600f), 0f) / 600f);
			hexColorMap.UpdateHex(tilePos.position, color);
		}
	}

    [With(typeof(TileData))]
    [With(typeof(TilePosition))]
    public class InsolationMapModeStartupSystem : InsolationMapMode {
        public InsolationMapModeStartupSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {}
    }

	[WhenChanged(typeof(TileData))]
    public class InsolationMapModeViewSystem : InsolationMapModeStartupSystem {
        public InsolationMapModeViewSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {}
    }
}