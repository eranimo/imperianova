extends ViewportContainer

onready var map = get_node("Viewport/WorldMap")

func set_tile(tile_pos: Vector2, tile_id: int):
	map.set_tile(tile_pos, tile_id)
