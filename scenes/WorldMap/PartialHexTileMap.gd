extends TileMap


var OceanBase = preload("res://assets/textures/tilesets/Ocean.png")
var GrasslandBase = preload("res://assets/textures/tilesets/Grass.png")

var base_ids = {
	MapData.Section.CENTER: 0,
	MapData.Section.EDGE_SE: 1,
	MapData.Section.EDGE_S: 2,
	MapData.Section.EDGE_SW: 3,
	MapData.Section.EDGE_NW: 4,
	MapData.Section.EDGE_N: 5,
	MapData.Section.EDGE_NE: 6,
}

var tileset = TileSet.new()
var tile_cache = {}

func _ready():
	self.tile_set = tileset

var terrain_type_base_tileset = {
	MapData.TerrainType.OCEAN: OceanBase,
	MapData.TerrainType.GRASSLAND: GrasslandBase,
}

func get_or_generate_tile(tile_pos: Vector2):
	var tile_bitmask = MapData.get_tile_bitmask(tile_pos)
	if tile_cache.has(tile_bitmask):
		return tile_cache[tile_bitmask]
	print("Generating tile ", tile_bitmask)
	# Generate texture for the current tile
	
	var terrain_type = MapData.tiles[tile_pos].terrain_type
	var base_texture: StreamTexture = terrain_type_base_tileset[terrain_type]
	var base_image = base_texture.get_data()

	var image = Image.new()
	image.create(64, 60, false, base_image.get_format())
	image.fill(Color(1.0, 1.0, 11.0))
	image.lock()
	image.set_pixel(0, 0, Color(1, 1, 1))
	image.blit_rect(
		base_image,
		Rect2(0, 0, 64, 60),
		Vector2(0, 0)
	)
	image.unlock()
	
	var err = image.save_png("res://debug_images/%d.png" % tile_bitmask)
	
	var texture = ImageTexture.new()
	texture.flags = Texture.FLAG_CONVERT_TO_LINEAR
	texture.create_from_image(image)
	
	var current_tile_id = tileset.get_last_unused_tile_id()
	tileset.create_tile(current_tile_id)
	tileset.tile_set_texture(current_tile_id, texture)
	tile_cache[tile_bitmask] = current_tile_id
	return tile_cache[tile_bitmask]

func set_tile(tile_pos):
	# pass
	# generate tile texture if it doesn't exist
	# get tileset texture id and set tilemap
	var tile_id = get_or_generate_tile(tile_pos)
	self.set_cellv(tile_pos, tile_id)
