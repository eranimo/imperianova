extends Node

var world_map: HexMap
signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)
signal tile_updated(tile_pos, data)


var pathfinder: AStar

var selected_tile = ReactiveState.new(null)
var selected_units = ReactiveSet.new()

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
	world_map = _map
	world_map.connect("tile_pressed", self, "_on_tile_pressed")
	world_map.connect("tile_hovered", self, "_on_tile_hovered")

	pathfinder = AStar.new()

func is_valid_pos(pos: Vector2):
	if pos.x < 0 or pos.y < 0:
		return false
	if pos.x >= MapData.game_world.map_width or pos.y >= MapData.game_world.map_height:
		return false
	return true

func _on_tile_pressed(tile_pos: Vector2):
	if not is_valid_pos(tile_pos):
		return
	var tile = MapData.get_tile(tile_pos)
	print("Tile pressed: %s (%s)" % [tile_pos, MapData.terrain_title[tile.terrain_type]])
	for dir in MapData.tile_neighbors[tile_pos]:
		var pos = MapData.tile_neighbors[tile_pos][dir]
		var n_tile = MapData.get_tile(pos)
		print("\tNeighbor %s:  %s (%s)" % [MapData.direction_titles[dir], pos, MapData.terrain_title[n_tile.terrain_type]])
	print('Terrain type: ', MapData.terrain_title[tile.terrain_type])
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


# Selected units

func clear_selected_units():
	for unit in selected_units.data:
		if EntitySystem.has_component(unit, "Selectable"):
			var selectable = EntitySystem.get_component(unit, "Selectable")
			selectable.is_selected = false
	selected_units.clear()

func select_unit(unit):
	print("Select unit ", unit)
	selected_units.add(unit)
	if EntitySystem.has_component(unit, "Selectable"):
		var selectable = EntitySystem.get_component(unit, "Selectable")
		selectable.is_selected = true

func deselect_unit(unit):
	print("Deselect unit ", unit)
	selected_units.remove(unit)
	if EntitySystem.has_component(unit, "Selectable"):
		var selectable = EntitySystem.get_component(unit, "Selectable")
		selectable.is_selected = false
