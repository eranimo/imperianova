extends TileMap

onready var terrain_tileset = preload("res://scenes/WorldMap/TerrainTileset.tres")

func _onready():
	tile_set = terrain_tileset

func set_tile(tile_pos: Vector2, local_pos: Vector2):
	# generate tile texture if it doesn't exist
	var tile_id = terrain_tileset.get_or_generate_tile(tile_pos)
	self.set_cellv(local_pos, tile_id)

func render():
	terrain_tileset.render()
