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

var texture
var image

var COLUMNS = 25
var ROWS = 25
var TILE_WIDTH = 64
var TILE_HEIGHT = 64
var PADDING = 20

func _ready():
	self.tile_set = tileset
	image = Image.new()
	image.create(
		ROWS * (TILE_WIDTH + PADDING),
		COLUMNS * (TILE_HEIGHT + PADDING),
		false,
		Image.FORMAT_RGBA8
	)
	texture = ImageTexture.new()
	texture.create_from_image(image, 0)
	for i in range(ROWS * COLUMNS):
		var dest = Vector2(
			(i % COLUMNS) * (TILE_WIDTH + PADDING),
			floor(i / COLUMNS) * (TILE_HEIGHT + PADDING)
		)
		tileset.create_tile(i)
		tileset.tile_set_texture(i, texture)
		tileset.tile_set_region(i, Rect2(dest.x, dest.y, TILE_WIDTH, TILE_HEIGHT))

var terrain_type_base_tileset = {
	MapData.TerrainType.OCEAN: OceanBase.get_data(),
	MapData.TerrainType.GRASSLAND: GrasslandBase.get_data(),
}

func get_or_generate_tile(tile_pos: Vector2):
	var tile_bitmask = int(MapData.get_tile_bitmask(tile_pos))
	if tile_cache.has(tile_bitmask):
		return tile_cache[tile_bitmask]
	
	var terrain_type = MapData.tiles[tile_pos].terrain_type
	var dest = Vector2(
		(tile_bitmask % COLUMNS) * (TILE_WIDTH + PADDING),
		floor(tile_bitmask / COLUMNS) * (TILE_HEIGHT + PADDING)
	)
	
	var base_image = terrain_type_base_tileset[terrain_type]
	image.blit_rect(
		base_image,
		Rect2(0, 0, TILE_WIDTH, TILE_HEIGHT),
		dest
	)

	tile_cache[tile_bitmask] = tile_bitmask
	return tile_bitmask

func set_tile(tile_pos):
	# generate tile texture if it doesn't exist
	var tile_id = get_or_generate_tile(tile_pos)
	self.set_cellv(tile_pos, tile_id)

func render():
	texture.set_data(image)
	# image.save_png("res://bin/test.png")
