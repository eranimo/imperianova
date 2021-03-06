extends Node

var map: HexMap
signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)
signal tile_updated(tile_pos, data)

var ReactiveState = preload("res://scripts/ReactiveState.gd")

var selected_tile = ReactiveState.new(null)

enum MapMode {
	NONE,
	HEIGHT,
	TEMPERATURE,
	RAINFALL,
}

var map_mode_titles = {
	MapMode.NONE: 'None',
	MapMode.HEIGHT: 'Height',
	MapMode.TEMPERATURE: 'Temperature',
	MapMode.RAINFALL: 'Rainfall',
}

var current_map_mode = ReactiveState.new(MapMode.NONE)

func set_map_mode(map_mode):
	current_map_mode.next(map_mode)

func connect_map(_map: HexMap):
	map = _map
	map.connect("tile_pressed", self, "_on_tile_pressed")
	map.connect("tile_hovered", self, "_on_tile_hovered")

func is_valid_pos(pos: Vector2):
	if pos.x < 0 or pos.y < 0:
		return false
	if pos.x >= MapData.world.map_width or pos.y >= MapData.world.map_height:
		return false
	return true

func _on_tile_pressed(tile_pos: Vector2):
	if not is_valid_pos(tile_pos):
		return
	print("Tile pressed: ", tile_pos)
	# map.set_cellv(tile_pos, 1)
	print('Terrain type: ', MapData.terrain_title[MapData.get_tile(tile_pos).terrain_type])
	emit_signal("tile_pressed", tile_pos)

	if selected_tile.value != null and selected_tile.value.is_equal_approx(tile_pos):
		selected_tile.next(null)
	else:
		selected_tile.next(tile_pos)

func _on_tile_hovered(tile_pos: Vector2, world_pos: Vector2):
	if not is_valid_pos(tile_pos):
		emit_signal("tile_hovered", null, null)
		return
	emit_signal("tile_hovered", tile_pos, world_pos)

func set_tile_development(tile_pos: Vector2, tile_development_id: int):
	assert(is_valid_pos(tile_pos), "Invalid tile")
	emit_signal("tile_updated", tile_pos, {
		"tile": tile_pos,
		"tile_development_id": tile_development_id,
	})
