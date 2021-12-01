using Godot;
using System;
using Hex;
using DefaultEcs;
using System.Collections.Generic;
using DefaultEcs.System;
using GameWorld;

public class HexColorMap {
	public readonly OffsetCoord size;

	public Image hexColorsImage;
	public ImageTexture hexColors;

	public HexColorMap(OffsetCoord size) {
		this.size = size;
		this.hexColorsImage = new Image();
		hexColorsImage.Create(size.Col, size.Row, false, Image.Format.Rgbaf);
		this.hexColors = new ImageTexture();
	}

	public void PreUpdate() {
		hexColorsImage.Lock();
	}

	public void UpdateHex(OffsetCoord pos, Color color) {
		hexColorsImage.SetPixel(pos.Col, pos.Row, color);
	}

	public void PostUpdate() {
		hexColorsImage.Unlock();
		hexColors.CreateFromImage(hexColorsImage);
	}

	public void Fill(Color color) {
		hexColorsImage.Lock();
		for(var row = 0; row < size.Row; row++) {
			for (var col = 0; col < size.Col; col++) {
				hexColorsImage.SetPixel(col, row, color);
			}
		}
		hexColorsImage.Unlock();
	}
}

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
			Color color = _gradient.Interpolate((float) decimal.Round((decimal) tileData.temperature, 2));
			hexColorMap.UpdateHex(tilePos.position, color);
		}
	}

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

public class WorldGrid : Polygon2D {
	private Polygon2D Grid;
	public int gridColumns;
	public int gridRows;
	public bool IsGridVisible = true;

	private GameWorld.World world;
	private WorldRenderer worldRenderer;
	private InputManager inputManager;
	private bool _hasRendered = false;

	[GameController] private GameController game;
	private Image hexColorsImage;
	private ImageTexture hexColors;

	public Dictionary<MapModes.MapMode, HexColorMap> mapModeColors;
	private Nullable<MapModes.MapMode> _lastUpdateMapModeType = null;
	private List<ISystem<GameDate>> initSystems;
	private List<ISystem<GameDate>> viewSystems;

	public ShaderMaterial shader {
		get { return (this.Material as ShaderMaterial); }
	}

	public override void _Ready() {
		this.ResolveDependencies();
		this.worldRenderer = (WorldRenderer) GetTree().Root.FindNode("WorldRenderer", true, false);
		inputManager = GetNode<InputManager>("/root/InputManager");
		inputManager.ActiveMapMode.Subscribe((MapModes.MapMode mapMode) => {
			if (_hasRendered) {
				UpdateHexColors(mapMode);
			}
		});
	}

	[GameInitHandler]
	public void OnGameInit() {
		initSystems = new List<ISystem<GameDate>>();
		viewSystems = new List<ISystem<GameDate>>();

		var entityManager = game.gameManager.entityManager;
		var worldInfo = entityManager.Get<WorldInfo>();
		this.mapModeColors = new Dictionary<MapModes.MapMode, HexColorMap>() {
			{ MapModes.MapMode.Terrain, new HexColorMap(worldInfo.size) },
			{ MapModes.MapMode.Temperature, new HexColorMap(worldInfo.size) },
			{ MapModes.MapMode.Rainfall, new HexColorMap(worldInfo.size) },
			{ MapModes.MapMode.Population, new HexColorMap(worldInfo.size) }
		};

		initSystems.Add(new MapModeSystems.TerrainMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.TemperatureMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.RainfallMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.PopulationMapModeStartupSystem(this, entityManager));

		viewSystems.Add(new MapModeSystems.PopulationMapModeViewSystem(this, entityManager));

		foreach (ISystem<GameDate> system in initSystems) {
			game.gameManager.CallInitSystem(system);
		}

		foreach (ISystem<GameDate> system in viewSystems) {
			game.gameManager.RegisterViewSystem(system);
		}

		this.Setup(worldInfo.size.Col, worldInfo.size.Row);
	}

	public override void _ExitTree() {
		foreach (ISystem<GameDate> system in viewSystems) {
			game.gameManager.UnregisterViewSystem(system);
		}
	}

	public void Setup(int TileWidth, int TileHeight) {
		_hasRendered = true;
		this.shader.SetShaderParam("zoom", inputManager.zoom);
		this.SetGridVisibility(true);

		this.gridColumns = TileWidth;
		this.gridRows =TileHeight;

		var containerSize = HexUtils.GetGridDimensions(this.gridColumns, this.gridRows);
		
		this.shader.SetShaderParam("hexSize", HexConstants.HEX_SIZE);
		this.shader.SetShaderParam("gridSize", new Vector2(this.gridColumns, this.gridRows));
		this.shader.SetShaderParam("containerSize", containerSize);
		this.Polygon = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, containerSize.y),
			new Vector2(containerSize.x, containerSize.y),
			new Vector2(containerSize.x, 0),
		};

		this.hexColorsImage = new Image();
		hexColorsImage.Create(this.gridColumns, this.gridRows, false, Image.Format.Rgbaf);
		this.hexColors = new ImageTexture();

		this.UpdateTerritoryMap();
		this.UpdateOccupiedMap();
		this.UpdateHexColors(inputManager.ActiveMapMode.Value);
	}

	public void UpdateHexColors(MapModes.MapMode mapModeType) {
		if (mapModeType == inputManager.ActiveMapMode.Value) {
			GD.PrintS("Set map mode", mapModeType);
			var hexColors = mapModeColors[mapModeType].hexColors;
			this.shader.SetShaderParam("hexColors", hexColors);
		}
	}

	private void UpdateTerritoryMap() {
		Image hexTerritoryColorImage = new Image();
		hexTerritoryColorImage.Create(this.gridColumns, this.gridRows, false, Image.Format.Rgbaf);

		hexTerritoryColorImage.Lock();

		hexTerritoryColorImage.SetPixel(1, 1, new Color(0.4f, 0.2f, 0.8f, 1 / 10_000f));
		hexTerritoryColorImage.SetPixel(1, 2, new Color(0.4f, 0.2f, 0.8f, 1 / 10_000f));
		hexTerritoryColorImage.SetPixel(2, 3, new Color(0.1f, 0.6f, 0.1f, 2 / 10_000f));
		hexTerritoryColorImage.SetPixel(3, 3, new Color(0.1f, 0.6f, 0.1f, 2 / 10_000f));
		hexTerritoryColorImage.SetPixel(3, 4, new Color(0.1f, 0.6f, 0.1f, 2 / 10_000f));

		hexTerritoryColorImage.Unlock();
		ImageTexture hexTerritoryColors = new ImageTexture();
		hexTerritoryColors.CreateFromImage(hexTerritoryColorImage);
		this.shader.SetShaderParam("hexTerritoryColor", hexTerritoryColors);
	}

	private void UpdateOccupiedMap() {
		Image hexOccupiedMapImage = new Image();
		hexOccupiedMapImage.Create(this.gridColumns, this.gridRows, false, Image.Format.Rgbaf);

		hexOccupiedMapImage.Lock();

		hexOccupiedMapImage.SetPixel(1, 1, new Color(0.1f, 0.6f, 0.1f, 1.0f));

		hexOccupiedMapImage.Unlock();
		ImageTexture hexOccupiedMapTexture = new ImageTexture();
		hexOccupiedMapTexture.CreateFromImage(hexOccupiedMapImage);
		this.shader.SetShaderParam("hexOccupiedColor", hexOccupiedMapTexture);
	}

	public void SetHighlightedHex(OffsetCoord hex) {
		this.shader.SetShaderParam("highlight", hex.ToAxial().AsVector());
	}

	public void SetSelectedHex(OffsetCoord hex) {
		this.shader.SetShaderParam("selectedHex", hex.ToAxial().AsVector());
	}

	public void SetGridVisibility(bool visible) {
		IsGridVisible = visible;
		this.shader.SetShaderParam("gridVisible", visible);
	}

	public void SetZoom(float zoom) {
		this.shader.SetShaderParam("zoom", zoom);
	}
}
