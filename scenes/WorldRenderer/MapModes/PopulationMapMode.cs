using Godot;
using System;
using DefaultEcs;
using System.Collections.Generic;
using DefaultEcs.System;
using GameWorld;

namespace MapModeSystems {
	public class PopulationMapModeSystem : MapModeSystem {
		public override MapModes.MapMode mapMode { get { return MapModes.MapMode.Population; } }
		private Gradient _gradient;
		private Dictionary<TilePosition, int> _hexSizes = new Dictionary<TilePosition, int>();

		public PopulationMapModeSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-population.tres") as Gradient;
		}

		protected override void ProcessEntity(Entity entity) {
			var popData = entity.Get<PopData>();
			var tilePos = entity.Get<TilePosition>();
			if (!_hexSizes.ContainsKey(tilePos)) {
				_hexSizes[tilePos] = 0;
			}
			_hexSizes[tilePos] += popData.size;
		}

		protected override void PostProcess() {
			GD.PrintS(GetType().Name, "PostProcess");
			foreach (TilePosition tilePos in _hexSizes.Keys) {
				var tilePopulation = _hexSizes[tilePos];
				Color color = _gradient.Interpolate(tilePopulation / 5_000f);
				hexColorMap.UpdateHex(tilePos.position, color);
			}
		}

		protected override void PreProcess() {
			_hexSizes.Clear();
			hexColorMap.Fill(_gradient.Interpolate(0));
		}
	}

	[With(typeof(PopData))]
	[With(typeof(TilePosition))]
	public class PopulationMapModeStartupSystem : PopulationMapModeSystem {
		public PopulationMapModeStartupSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {}
	}

	[WhenChanged(typeof(PopData))]
	public class PopulationMapModeViewSystem : PopulationMapModeStartupSystem {
		public PopulationMapModeViewSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {}
	}
}