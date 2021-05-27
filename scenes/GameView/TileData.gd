extends VBoxContainer

func _ready():
	MapManager.connect("tile_hovered", self, "_on_tile_hovered")

func _on_tile_hovered(tile_pos, world_pos):
	if tile_pos == null:
		self.hide()
	else:
		self.show()
		$HBoxContainer/TilePos.text = "%d, %d" % [tile_pos.x, tile_pos.y]
