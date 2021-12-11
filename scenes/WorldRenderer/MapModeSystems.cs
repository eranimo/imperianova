using Godot;
using System;
using DefaultEcs;
using System.Collections.Generic;
using DefaultEcs.System;
using GameWorld;

namespace MapModeSystems {
	public abstract class MapModeSystem : AEntitySetSystem<GameDate> {
		public abstract MapModes.MapMode mapMode { get; }
		public WorldGrid Grid { get; }

		protected HexColorMap hexColorMap;

		public MapModeSystem(WorldGrid grid, DefaultEcs.World world) : base(world) {
			Grid = grid;
			this.hexColorMap = Grid.mapModeColors[mapMode];
		}

		protected override void Update(GameDate date, ReadOnlySpan<Entity> entities) {
			PreProcess();
			hexColorMap.PreUpdate();
			foreach (Entity entity in entities) {
				ProcessEntity(entity);
			}
			PostProcess();
			hexColorMap.PostUpdate();
			Grid.UpdateHexColors(mapMode);
		}

		protected virtual void ProcessEntity(Entity entity) {}
		protected virtual void PostProcess() {}
		protected virtual void PreProcess() {}
	}

	[With(typeof(TileData))]
	[With(typeof(TilePosition))]
	public class TerrainMapModeStartupSystem : MapModeSystem {
		public override MapModes.MapMode mapMode { get { return MapModes.MapMode.Terrain; } }
		public TerrainMapModeStartupSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {}

		protected override void ProcessEntity(Entity entity) {
			var tileData = entity.Get<TileData>();
			var tilePos = entity.Get<TilePosition>();
			Color color = TileConstants.TerrainColors[tileData.terrainType];
			hexColorMap.UpdateHex(tilePos.position, color);
		}
	}

	[With(typeof(TileData))]
	[With(typeof(TilePosition))]
	public class TemperatureMapModeStartupSystem : MapModeSystem {
		public override MapModes.MapMode mapMode { get { return MapModes.MapMode.Temperature; } }
		private Gradient _gradient;
		public TemperatureMapModeStartupSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-temperature.tres") as Gradient;
		}

		protected override void ProcessEntity(Entity entity) {
			var tileData = entity.Get<TileData>();
			var tilePos = entity.Get<TilePosition>();
			Color color = _gradient.Interpolate((float) decimal.Round((decimal) tileData.temperature, 2));
			hexColorMap.UpdateHex(tilePos.position, color);
		}
	}

	[With(typeof(TileData))]
	[With(typeof(TilePosition))]
	public class RainfallMapModeStartupSystem : MapModeSystem {
		public override MapModes.MapMode mapMode { get { return MapModes.MapMode.Rainfall; } }
		private Gradient _gradient;
		public RainfallMapModeStartupSystem(WorldGrid grid, DefaultEcs.World world) : base(grid, world) {
			_gradient = ResourceLoader.Load("res://resources/colormaps/mapmode-rainfall.tres") as Gradient;
		}

		protected override void ProcessEntity(Entity entity) {
			var tileData = entity.Get<TileData>();
			var tilePos = entity.Get<TilePosition>();
			Color color = _gradient.Interpolate((float) decimal.Round((decimal) tileData.rainfall, 2));
			hexColorMap.UpdateHex(tilePos.position, color);
		}
	}
}