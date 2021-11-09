extends VBoxContainer

func _ready():
	InputManager.connect("TileHovered", self, "_on_tile_hovered")

func _on_tile_hovered(tile):
	if not WorldData.IsValidTile(tile.x, tile.y):
		self.hide()
	else:
		self.show()
		var tile_data = WorldData.GetTile(tile.x, tile.y)
		$TileCoord.value = "%d, %d" % [tile.x, tile.y]
		$Terrain.value = MapData.terrain_title[tile_data.terrain_type]
		$Height.value = tile_data.height
		$Temperature.value = tile_data.temperature
		$Rainfall.value = tile_data.rainfall
