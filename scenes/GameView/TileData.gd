extends VBoxContainer

func _ready():
	MapManager.connect("tile_hovered", self, "_on_tile_hovered")

func _on_tile_hovered(tile_pos, world_pos):
	if tile_pos == null:
		self.hide()
	else:
		self.show()
		var tile_data = MapData.get_tile(tile_pos)
		$TileCoord.value = "%d, %d" % [tile_pos.x, tile_pos.y]
		$Terrain.value = MapData.terrain_title[MapData.get_tile(tile_pos).terrain_type]
		$Height.value = tile_data.height
		$Temperature.value = tile_data.temperature
		$Rainfall.value = tile_data.rainfall
