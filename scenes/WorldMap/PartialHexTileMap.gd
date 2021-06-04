extends TileMap


var OceanBase = preload("res://assets/textures/tilesets/Ocean.png")
var GrasslandBase = preload("res://assets/textures/tilesets/Grass.png")
var DesertBase = preload("res://assets/textures/tilesets/Desert.png")
var OceanGrasslandTrans2 = preload("res://assets/textures/tilesets/Ocean-Grass.png")
var OceanDesertTrans2 = preload("res://assets/textures/tilesets/Ocean-Desert.png")
var OceanGrasslandDesertTrans3 = preload("res://assets/textures/tilesets/OceanGrass-Desert.png")
var GrasslandDesertTran2 = preload("res://assets/textures/tilesets/Grass-Desert.png")

var base_column_ids = {
	MapData.Section.CENTER: 0,
	MapData.Section.EDGE_SE: 1,
	MapData.Section.EDGE_S: 2,
	MapData.Section.EDGE_SW: 3,
	MapData.Section.EDGE_NW: 4,
	MapData.Section.EDGE_N: 5,
	MapData.Section.EDGE_NE: 6,
}

var transition_column_ids = {
	MapData.Section.EDGE_SE: 0,
	MapData.Section.EDGE_S: 1,
	MapData.Section.EDGE_SW: 2,
	MapData.Section.EDGE_NW: 3,
	MapData.Section.EDGE_N: 4,
	MapData.Section.EDGE_NE: 5,
}

var terrain_type_base_tileset = {
	MapData.TerrainType.OCEAN: OceanBase.get_data(),
	MapData.TerrainType.GRASSLAND: GrasslandBase.get_data(),
	MapData.TerrainType.DESERT: DesertBase.get_data(),
}

var transition_2_tileset = {
	MapData.TerrainType.OCEAN: {
		MapData.TerrainType.GRASSLAND: OceanGrasslandTrans2.get_data(),
		MapData.TerrainType.DESERT: OceanDesertTrans2.get_data(),
	},
	MapData.TerrainType.GRASSLAND: {
		MapData.TerrainType.DESERT: GrasslandDesertTran2.get_data(),
	}
}

var transition_3_tileset = {
	MapData.TerrainType.OCEAN: {
		MapData.TerrainType.GRASSLAND: {
			MapData.TerrainType.DESERT: OceanGrasslandDesertTrans3.get_data()
		}
	}
}

var tileset = TileSet.new()
var tile_cache = {}

var texture
var image

# generated tileset max size
# if the number of tile section combinations increases over this number
# there will be an error
var COLUMNS = 100
var ROWS = 100
var TILE_WIDTH = 64
var TILE_HEIGHT = 60
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

func _get_source_tile(id, columns, padding=0):
	return Vector2(
		(id % columns) * (TILE_WIDTH + padding),
		floor(id / columns) * (TILE_HEIGHT + padding)
	)

func get_transtion_two_tile_id(row, section):
	var section_column_id = transition_column_ids[section]
	return ((row - 1) * 6) + section_column_id

func _render_edge(tile_pos, dest, section):
	var tile_data = MapData.tiles[tile_pos]
	var section_column_id = base_column_ids[section]
	var dir = MapData.section_to_direction[section]
	var terrain_type = tile_data.terrain_type
	if not terrain_type_base_tileset.has(terrain_type):
		print("Missing tileset for terrain type ", terrain_type)
		return
	var base_image = terrain_type_base_tileset[terrain_type]
	var adj_dir_1 = MapData.direction_clockwise[dir]
	var adj_dir_2 = MapData.direction_counter_clockwise[dir]
	var edge_terrain = tile_data.edge[dir]
	var adj1_terrain = tile_data.edge[adj_dir_1]
	var adj2_terrain = tile_data.edge[adj_dir_2]
	if MapData.has_transition(terrain_type, tile_data.edge[dir]):
		for transition_terrain in MapData.terrain_transitions[terrain_type]:
			var transition_image = transition_2_tileset[terrain_type][transition_terrain]
			if edge_terrain == transition_terrain and \
				adj1_terrain == terrain_type and \
				adj2_terrain == transition_terrain:
				# Row 4: edge = secondary; adj1 = primary; adj2 = secondary
				var rect = _get_source_tile(get_transtion_two_tile_id(4, section), 6, 0)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
			elif edge_terrain == transition_terrain and \
				adj1_terrain == transition_terrain and \
				adj2_terrain == terrain_type:
				# Row 5: edge = secondary; adj1 = secondary; adj2 = primary
				var rect = _get_source_tile(get_transtion_two_tile_id(5, section), 6)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
			elif edge_terrain == transition_terrain and \
				adj1_terrain == transition_terrain and \
				adj2_terrain == transition_terrain:
				# Row 6: edge = secondary; adj1 = secondary; adj2 = secondary
				var rect = _get_source_tile(get_transtion_two_tile_id(6, section), 6)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
			elif edge_terrain == transition_terrain and \
				adj1_terrain == terrain_type and \
				adj2_terrain == terrain_type:
				# Row 7: edge = secondary; adj1 = primary; adj2 = primary
				var rect = _get_source_tile(get_transtion_two_tile_id(7, section), 6)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
	elif MapData.has_transition(terrain_type, tile_data.edge[adj_dir_1]) or \
		MapData.has_transition(terrain_type, tile_data.edge[adj_dir_2]):
		for transition_terrain in MapData.terrain_transitions[terrain_type]:
			var transition_image = transition_2_tileset[terrain_type][transition_terrain]
			if edge_terrain == terrain_type and \
				adj1_terrain == transition_terrain and \
				adj2_terrain == terrain_type:
				# Row 1: edge = primary; adj1 = secondary; adj2 = primary
				var rect = _get_source_tile(get_transtion_two_tile_id(1, section), 6)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
			elif edge_terrain == terrain_type and \
				adj1_terrain == terrain_type and \
				adj2_terrain == transition_terrain:
				# Row 2: edge = primary; adj1 = primary; adj2 = secondary
				var rect = _get_source_tile(get_transtion_two_tile_id(2, section), 6)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
			elif edge_terrain == terrain_type and \
				adj1_terrain == transition_terrain and \
				adj2_terrain == transition_terrain:
				# Row 3: edge = primary; adj1, adj2 = secondary
				var rect = _get_source_tile(get_transtion_two_tile_id(3, section), 6)
				image.blit_rect_mask(
					transition_image,
					transition_image,
					Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
					dest
				)
	else:
		var rect = _get_source_tile(section_column_id, 7)
		image.blit_rect_mask(
			base_image,
			base_image,
			Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT),
			dest
		)

func _render_center(tile_pos, dest):
	var tile_data = MapData.tiles[tile_pos]
	var terrain_type = tile_data.terrain_type
	if not terrain_type_base_tileset.has(terrain_type):
		print("Missing tileset for terrain type ", terrain_type)
		return
	var base_image = terrain_type_base_tileset[terrain_type]
	image.blit_rect_mask(
		base_image,
		base_image,
		Rect2(0, 0, TILE_WIDTH, TILE_HEIGHT),
		dest
	)

func get_or_generate_tile(tile_pos: Vector2):
	var tile_bitmask = int(MapData.get_tile_bitmask(tile_pos))
	if tile_cache.has(tile_bitmask):
		return tile_cache[tile_bitmask]
	
	var dest = Vector2(
		(tile_bitmask % COLUMNS) * (TILE_WIDTH + PADDING),
		floor(tile_bitmask / COLUMNS) * (TILE_HEIGHT + PADDING)
	)
	
	# Render from the top down:
	# N, NE, NW, CENTER, SW, SE, S
	
	_render_edge(tile_pos, dest, MapData.Section.EDGE_N)
	_render_edge(tile_pos, dest, MapData.Section.EDGE_NE)
	_render_edge(tile_pos, dest, MapData.Section.EDGE_NW)
	_render_center(tile_pos, dest)
	_render_edge(tile_pos, dest, MapData.Section.EDGE_SW)
	_render_edge(tile_pos, dest, MapData.Section.EDGE_SE)
	_render_edge(tile_pos, dest, MapData.Section.EDGE_S)

	tile_cache[tile_bitmask] = tile_bitmask
	return tile_bitmask

func set_tile(tile_pos):
	# generate tile texture if it doesn't exist
	var tile_id = get_or_generate_tile(tile_pos)
	self.set_cellv(tile_pos, tile_id)

func render():
	texture.set_data(image)
	# image.save_png("res://bin/test.png")
