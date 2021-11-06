using Godot;
using System;

public class WorldRenderer : Node2D {
	private Polygon2D Grid;

	public override void _Ready() {
		this.Grid = (Polygon2D) FindNode("Grid");
		this.SetWorld();
	}

	public void SetWorld() {
		int width = 10;
		int height = 10;
		var shader = (this.Grid.Material as ShaderMaterial);
		shader.SetShaderParam("gridSize", new Vector2(width, height));
		shader.SetShaderParam("hexSize", 32.0);
		shader.SetShaderParam("highlight", new Vector2(2, 2));

		// ImageTexture hexColors = new ImageTexture();
		// Image hexColorsImage = new Image();
		// hexColorsImage.Lock();
		// hexColorsImage.Create(width, height, false, Image.Format.Rgb8);
		// for (var x = 0; x < width; x++) {
		// 	for (var y = 0; y < height; y++) {
		// 		hexColorsImage.SetPixel(x, y, new Color(0.1f * x, 0.1f * y, 0.5f, 1.0f));
		// 	}
		// }
		// hexColorsImage.Unlock();
		// hexColors.CreateFromImage(hexColorsImage);
		// shader.SetShaderParam("hexColors", hexColors);
	}
}
