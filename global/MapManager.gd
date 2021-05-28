extends Node

var map: HexMap
var map_width
var map_height

signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)

func connect_map(_map: HexMap):
	map = _map
	map.connect("tile_pressed", self, "_on_tile_pressed")
	map.connect("tile_hovered", self, "_on_tile_hovered")

func connect_world(world):
	map_width = world.map_width
	map_height = world.map_height

func is_valid_pos(pos: Vector2):
	if pos.x < 0 or pos.y < 0:
		return false
	if pos.x > map_width or pos.y > map_height:
		return false
	return true

func _on_tile_pressed(tile_pos: Vector2):
	if not is_valid_pos(tile_pos):
		return
	print("Tile pressed ", tile_pos)
	map.set_cellv(tile_pos, 1)
	emit_signal("tile_pressed", tile_pos)

func _on_tile_hovered(tile_pos: Vector2, world_pos: Vector2):
	if not is_valid_pos(tile_pos):
		emit_signal("tile_hovered", null, null)
		return
	emit_signal("tile_hovered", tile_pos, world_pos)
