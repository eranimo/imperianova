extends VBoxContainer

func _ready():
	MapManager.connect("tile_hovered", self, "_on_tile_hovered")

func _on_tile_hovered(tile_pos, world_pos):
	if tile_pos == null:
		self.hide()
	else:
		self.show()
		$HBoxContainer/Value.text = "%d, %d" % [tile_pos.x, tile_pos.y]
		$HBoxContainer2/Value.text = MapData.terrain_title[MapData.get_tile(tile_pos).terrain_type]
		$HBoxContainer3/Value.text = str(MapData.get_tile(tile_pos).height)
		$HBoxContainer4/Value.text = str(MapData.get_tile(tile_pos).temperature)
		$HBoxContainer5/Value.text = str(MapData.get_tile(tile_pos).rainfall)
