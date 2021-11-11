using Godot;
using System;
using Hex;

public class WorldGrid : Polygon2D {
	private Polygon2D Grid;
	public int gridColumns;
	public int gridRows;

	private GameWorld.World world;
	private InputManager inputManager;
	private bool _hasRendered = false;

	public ShaderMaterial shader {
		get {
			return (this.Material as ShaderMaterial);
		}
	}

	public override void _Ready() {
		inputManager = GetNode<InputManager>("/root/InputManager");
		inputManager.ActiveMapMode.Subscribe((MapModes.MapMode mapMode) => {
			if (_hasRendered) {
				UpdateHexColors(mapMode);
			}
		});
	}

	public void Render(GameWorld.World world) {
		_hasRendered = true;
		this.world = world;
		GD.PrintS("[WorldGrid] Render world:", this.world.TileWidth, this.world.TileHeight);
		this.SetGridVisibility(true);
		this.SetupGrid();
		this.UpdateTerritoryMap();
		this.UpdateHexColors(inputManager.ActiveMapMode.Value);
	}

	private void SetupGrid() {
		this.gridColumns = this.world.TileWidth;
		this.gridRows = this.world.TileHeight;

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
	}

	private void UpdateHexColors(MapModes.MapMode mapModeType) {
		GD.PrintS("Set map mode", mapModeType);
		Image hexColorsImage = new Image();
		hexColorsImage.Create(this.gridColumns, this.gridRows, false, Image.Format.Rgbaf);
		hexColorsImage.Lock();
		var mapMode = MapModes.mapModes[mapModeType];
		foreach (GameWorld.Tile tile in this.world.Tiles) {
			Color hexColor = mapMode.GetTileColor(tile);
			hexColorsImage.SetPixel(tile.position.Col, tile.position.Row, hexColor);
		}

		hexColorsImage.Unlock();
		ImageTexture hexColors = new ImageTexture();
		hexColors.CreateFromImage(hexColorsImage);
		this.shader.SetShaderParam("hexColors", hexColors);
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

	public void SetHighlightedHex(OffsetCoord hex) {
		this.shader.SetShaderParam("highlight", hex.ToAxial().AsVector());
	}

	public void SetSelectedHex(OffsetCoord hex) {
		this.shader.SetShaderParam("selectedHex", hex.ToAxial().AsVector());
	}

	public void SetGridVisibility(bool visible) {
		this.shader.SetShaderParam("gridVisible", visible);
	}
}
