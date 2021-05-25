extends ViewportContainer

onready var map = $Viewport/Map

func get_tile_for_cell(world_map: WorldMap, pos: Vector2):
	var height = world_map.get_cell(pos)
	if height < 0.5:
		return 0
	else:
		return 1

func setup_map(world_map: WorldMap) -> void:
	for x in range(world_map.map_width):
		for y in range(world_map.map_height):
			var pos = Vector2(x, y)
			var index = get_tile_for_cell(world_map, pos)
			map.set_cellv(Vector2(x, y), index)

func set_tile(tile_pos: Vector2, tile_id: int):
	map.set_cellv(tile_pos, tile_id)
