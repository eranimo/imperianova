extends ViewportContainer

onready var map = $Viewport/Map

func set_tile(tile_pos: Vector2, tile_id: int):
	map.set_cellv(tile_pos, tile_id)
