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
		this.mapModeColors = new Dictionary<MapModes.MapMode, HexColorMap>();
		foreach (int i in Enum.GetValues(typeof(MapModes.MapMode))) {
			this.mapModeColors[(MapModes.MapMode) i] = new HexColorMap(worldInfo.size);
		}

		initSystems.Add(new MapModeSystems.TerrainMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.TemperatureMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.RainfallMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.PopulationMapModeStartupSystem(this, entityManager));
		initSystems.Add(new MapModeSystems.InsolationMapModeStartupSystem(this, entityManager));

		viewSystems.Add(new MapModeSystems.PopulationMapModeViewSystem(this, entityManager));
		viewSystems.Add(new MapModeSystems.InsolationMapModeViewSystem(this, entityManager));

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
		this.shader.SetShaderParam("showHexArrows", false);
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
