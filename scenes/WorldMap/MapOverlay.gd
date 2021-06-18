extends Node2D

const ALPHA = 0.75
var overlay_tiles = {}

func _ready():
	MapManager.current_map_mode.subscribe(self, '_map_mode_change')

func _map_mode_change(map_mode):
	if map_mode == MapManager.MapMode.NONE:
		for pos in overlay_tiles:
			overlay_tiles[pos].visible = false
		return

	for pos in overlay_tiles:
		var color

		if map_mode == MapManager.MapMode.HEIGHT:
			var height = MapData.world_data[pos].height
			color = Color(height / 255.0, height / 255.0, height / 255.0)
		
		set_overlay_tile_color(pos, color)

func add_overlay_tile(pos: Vector2, overlay: Sprite):
	overlay.visible = false
	overlay_tiles[pos] = overlay
	add_child(overlay)

func set_overlay_tile_color(pos: Vector2, color: Color):
	var overlay = overlay_tiles[pos]
	overlay.visible = true
	color.a = ALPHA
	overlay.modulate = color
