using Godot;
using System;
using Hex;

public class WorldGrid : Polygon2D {
	private Polygon2D Grid;
	public int gridColumns;
	public int gridRows;

	private GameWorld.World world;

	public ShaderMaterial shader {
		get {
			return (this.Material as ShaderMaterial);
		}
	}

	public override void _Ready() {
	}

	public void Render(GameWorld.World world) {
		this.world = world;
		GD.PrintS("[WorldGrid] Render world:", this.world.TileWidth, this.world.TileHeight);
		this.SetGridVisibility(true);
		this.SetupGrid();
		this.UpdateHexColors();
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

	private void UpdateHexColors() {
		Image hexColorsImage = new Image();
		hexColorsImage.Create(this.gridColumns, this.gridRows, false, Image.Format.Rgbaf);
		hexColorsImage.Lock();
		foreach (GameWorld.Tile tile in this.world.Tiles) {
			Color hexColor = GameWorld.TileConstants.TerrainColors[tile.terrainType];
			hexColorsImage.SetPixel(tile.position.Col, tile.position.Row, hexColor);
		}

		hexColorsImage.Unlock();
		ImageTexture hexColors = new ImageTexture();
		hexColors.CreateFromImage(hexColorsImage);
		this.shader.SetShaderParam("hexColors", hexColors);
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
