using Godot;
using System;
using Hex;

public class WorldGrid : Polygon2D {
	private Polygon2D Grid;
	public int gridColumns;
	public int gridRows;

	public ShaderMaterial shader {
		get {
			return (this.Material as ShaderMaterial);
		}
	}

	public override void _Ready() {
	}

	public void SetupGrid(int rows, int cols) {
		this.gridColumns = cols;
		this.gridRows = rows;

		var lastHexPoint = Hex.HexUtils.HexToPixel(new OffsetCoord(this.gridRows, this.gridColumns));
		var gridWidth = lastHexPoint.x;
		var gridHeight = lastHexPoint.y;
		GD.PrintS("Hex container size", gridWidth, gridHeight);
		
		this.shader.SetShaderParam("hexSize", HexConstants.HEX_SIZE);
		this.shader.SetShaderParam("gridSize", new Vector2(this.gridColumns, this.gridRows));
		this.shader.SetShaderParam("containerSize", new Vector2(gridWidth, gridHeight));
		this.Polygon = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, gridHeight),
			new Vector2(gridWidth, gridHeight),
			new Vector2(gridWidth, 0),
		};
	}

	public void Setup() {
		var shader = (this.Material as ShaderMaterial);
		Image hexColorsImage = new Image();
		hexColorsImage.Create(this.gridRows, this.gridColumns, false, Image.Format.Rgbaf);
		hexColorsImage.Lock();
		for (var y = 0; y < this.gridRows; y++) {
			for (var x = 0; x < this.gridColumns; x++) {
				Color hexColor = new Color(0.1f * x, 0.1f * y, 0.5f, 1.0f);
				hexColorsImage.SetPixel(x, y, hexColor);
			}
		}
		hexColorsImage.Unlock();
		ImageTexture hexColors = new ImageTexture();
		hexColors.CreateFromImage(hexColorsImage);
		shader.SetShaderParam("hexColors", hexColors);
	}

	public void setHighlight(OffsetCoord hex) {
		this.shader.SetShaderParam("highlight", hex.ToAxial().AsVector());
	}
}
