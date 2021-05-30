extends TileMap

var tile_cache
var OceanBase = preload("res://assets/textures/tilesets/Ocean.png")
var GrasslandBase = preload("res://assets/textures/tilesets/Grass.png")
var OceanGrassTransition = preload("res://assets/textures/tilesets/Ocean-Grass.png")

var terrain_type_images = {
	MapData.TerrainType.OCEAN: {
		"base": OceanBase
	},
	MapData.TerrainType.GRASSLAND: {
		"base": GrasslandBase
	}
}

func _ready():
	tile_cache = {}

func set_tile(tile_pos, terrain_type):
	# generate tile texture if it doesn't exist
	# get tileset texture id and set tilemap
	pass
