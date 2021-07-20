extends HexMap
class_name SectionalTilemap

onready var HexShape = preload("res://assets/textures/hex-shape.png")

func render():
	update()

func _draw_tile_center(
	_tile_pos: Vector2,
	_dest: Rect2,
	_tile_data
):
	assert(false, "Needs implementation")

func _draw_tile_edge(
	_tile_pos: Vector2,
	_dest: Rect2,
	_tile_data,
	_section
):
	assert(false, "Needs implementation")

func _draw():
	var time_start = OS.get_ticks_msec()
	for chunk_tile in get_parent().chunk_tiles:

		var position = map_to_world(chunk_tile.local)
		var dest = Rect2(position, MapTilesets.TILE_SIZE)
		var tile_data = MapData.get_tile(chunk_tile.global)
		if not MapTilesets.terrain_type_base_tileset.has(tile_data.terrain_type):
			print("Missing tileset for terrain type ", tile_data.terrain_type)
			return
		_draw_tile_edge(chunk_tile.global, dest, tile_data, MapData.Section.EDGE_N)
		_draw_tile_edge(chunk_tile.global, dest, tile_data, MapData.Section.EDGE_NE)
		_draw_tile_edge(chunk_tile.global, dest, tile_data, MapData.Section.EDGE_NW)
		_draw_tile_center(chunk_tile.global, dest, tile_data)
		_draw_tile_edge(chunk_tile.global, dest, tile_data, MapData.Section.EDGE_SW)
		_draw_tile_edge(chunk_tile.global, dest, tile_data, MapData.Section.EDGE_SE)
		_draw_tile_edge(chunk_tile.global, dest, tile_data, MapData.Section.EDGE_S)
	
	print("Render chunk %s terrain (%d ms)" % [str(get_parent().chunk_position), OS.get_ticks_msec() - time_start])
		
