extends TileSet
class_name TerrainTileset


var OceanBase = preload("res://assets/textures/tilesets/Ocean.png").get_data()
var GrasslandBase = preload("res://assets/textures/tilesets/Grass.png").get_data()
var DesertBase = preload("res://assets/textures/tilesets/Desert.png").get_data()
var ForestBase = preload("res://assets/textures/tilesets/Forest.png").get_data()

var OceanGrasslandTrans2 = preload("res://assets/textures/tilesets/Ocean-Grass.png").get_data()
var OceanDesertTrans2 = preload("res://assets/textures/tilesets/Ocean-Desert.png").get_data()
var OceanForestTrans2 = preload("res://assets/textures/tilesets/Ocean-Forest.png").get_data()
var GrasslandDesertTrans2 = preload("res://assets/textures/tilesets/Grass-Desert.png").get_data()
var GrasslandForestTrans2 = preload("res://assets/textures/tilesets/Grass-Forest.png").get_data()
var ForestDesertTran2 = preload("res://assets/textures/tilesets/ForestGrass-Desert.png").get_data()

var OceanGrasslandDesertTrans3 = preload("res://assets/textures/tilesets/OceanGrass-Desert.png").get_data()
var GrasslandForestDesertTrans3 = preload("res://assets/textures/tilesets/Grassland-Forest(Desert).png").get_data()
var OceanGrasslandForestTrans3 = preload("res://assets/textures/tilesets/Ocean-Grassland(Forest).png").get_data()
var OceanForestDesertTrans3 = preload("res://assets/textures/tilesets/Ocean-Forest(Desert).png").get_data()

# features
var FeatureForestCenter = preload("res://assets/textures/tilesets/Feature-Forest-Center.png").get_data()
var FeatureForestEdges = preload("res://assets/textures/tilesets/Trees-Hex.png").get_data()

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
	MapData.TerrainType.OCEAN: OceanBase,
	MapData.TerrainType.GRASSLAND: GrasslandBase,
	MapData.TerrainType.DESERT: DesertBase,
	MapData.TerrainType.FOREST: ForestBase,
}

var transition_2_tileset = {
	MapData.TerrainType.OCEAN: {
		MapData.TerrainType.GRASSLAND: OceanGrasslandTrans2,
		MapData.TerrainType.DESERT: OceanDesertTrans2,
		MapData.TerrainType.FOREST: OceanForestTrans2,
	},
	MapData.TerrainType.GRASSLAND: {
		MapData.TerrainType.DESERT: GrasslandDesertTrans2,
		MapData.TerrainType.FOREST: GrasslandForestTrans2,
	},
	MapData.TerrainType.FOREST: {
		MapData.TerrainType.DESERT: ForestDesertTran2
	}
}

var transition_3_tileset = {
	MapData.TerrainType.OCEAN: {
		MapData.TerrainType.GRASSLAND: {
			MapData.TerrainType.DESERT: OceanGrasslandDesertTrans3,
			MapData.TerrainType.FOREST: OceanGrasslandForestTrans3,
		},
		MapData.TerrainType.FOREST: {
			MapData.TerrainType.DESERT: OceanForestDesertTrans3,
		}
	},
	MapData.TerrainType.GRASSLAND: {
		MapData.TerrainType.FOREST: {
			MapData.TerrainType.DESERT: GrasslandForestDesertTrans3,
		}
	}
}

# generated tileset max size
# if the number of tile section combinations increases over this number
# there will be an error
var COLUMNS = 50
var ROWS = 25
var TILE_WIDTH = 64
var TILE_HEIGHT = 60
var PADDING = 20

var tile_cache = {}
var missing_tilesets = {}
var texture
var image

func _init():
	image = Image.new()
	image.create(
		COLUMNS * (TILE_WIDTH + PADDING),
		ROWS * (TILE_HEIGHT + PADDING),
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
		create_tile(i)
		tile_set_texture(i, texture)
		tile_set_region(i, Rect2(dest.x, dest.y, TILE_WIDTH, TILE_HEIGHT))

func render():
	texture.set_data(image)
	# print(missing_tilesets)
	# image.save_png("res://bin/test.png")

func _get_transition_image(primary, secondary, tertiary=null):
	if tertiary:
		if not transition_3_tileset.has(primary) or \
			not transition_3_tileset.get(primary).has(secondary) or \
			not transition_3_tileset.get(primary).get(secondary).has(tertiary):
			var key = '%s-%s-%s' % [MapData.terrain_title[primary], MapData.terrain_title[secondary], MapData.terrain_title[tertiary]]
			missing_tilesets[key] = missing_tilesets.get(key, 0) + 1
			return null
		else:
			return transition_3_tileset[primary][secondary][tertiary]
	else:
		if not transition_2_tileset.has(primary) or \
			not transition_2_tileset.get(primary).has(secondary):
			var key = '%s-%s' % [MapData.terrain_title[primary], MapData.terrain_title[secondary]]
			missing_tilesets[key] = missing_tilesets.get(key, 0) + 1
			return null
		else:
			return transition_2_tileset[primary][secondary]

func _get_source_tile(id, columns, padding=0, tile_width=TILE_WIDTH, tile_height=TILE_HEIGHT):
	return Vector2(
		(id % columns) * (tile_width + padding),
		floor(id / columns) * (tile_height + padding)
	)

func get_transtion_tile_id(row, section):
	var section_column_id = transition_column_ids[section]
	return ((row - 1) * 6) + section_column_id

func _render_edge(tile_pos, dest, section):
	var tile_data = MapData.get_tile(tile_pos)
	var tile_edges = MapData.get_tile_edges(tile_pos)
	var section_column_id = base_column_ids[section]
	var dir = MapData.section_to_direction[section]
	var terrain_type = tile_data.terrain_type
	if not terrain_type_base_tileset.has(terrain_type):
		print("Missing tileset for terrain type ", terrain_type)
		return
	var base_image = terrain_type_base_tileset[terrain_type]
	var adj_dir_1 = MapData.direction_clockwise[dir]
	var adj_dir_2 = MapData.direction_counter_clockwise[dir]
	var edge_terrain = tile_edges[dir]
	var adj1_terrain = tile_edges[adj_dir_1]
	var adj2_terrain = tile_edges[adj_dir_2]
	
	var has_trans_edge = MapData.has_transition(terrain_type, tile_edges[dir])
	var has_trans_adj1 = MapData.has_transition(terrain_type, tile_edges[adj_dir_1])
	var has_trans_adj2 = MapData.has_transition(terrain_type, tile_edges[adj_dir_2])

	var source_img = null
	var rect = null
	if has_trans_edge:
		if has_trans_adj2 and adj1_terrain != edge_terrain and adj2_terrain == edge_terrain:
			if MapData.has_transition(edge_terrain, adj1_terrain):
				# 3-trans Row 6
				rect = _get_source_tile(get_transtion_tile_id(6, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain, adj1_terrain)
			else:
				# Row 4: edge = secondary; adj1 = primary; adj2 = secondary
				rect = _get_source_tile(get_transtion_tile_id(4, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain)
		elif has_trans_adj1 and adj1_terrain == edge_terrain and adj2_terrain != edge_terrain:
			if MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 7
				rect = _get_source_tile(get_transtion_tile_id(7, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain, adj2_terrain)
			else:
				# Row 5: edge = secondary; adj1 = secondary; adj2 = primary
				rect = _get_source_tile(get_transtion_tile_id(5, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain)
		elif has_trans_adj1 and has_trans_adj2 and adj1_terrain == edge_terrain and adj2_terrain == edge_terrain:
			# Row 6: edge = secondary; adj1 = secondary; adj2 = secondary
			rect = _get_source_tile(get_transtion_tile_id(6, section), 6)
			source_img = _get_transition_image(terrain_type, edge_terrain)
		elif adj1_terrain != edge_terrain and adj2_terrain != edge_terrain:
			if MapData.has_transition(edge_terrain, adj1_terrain) and not MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 1
				rect = _get_source_tile(get_transtion_tile_id(1, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain, adj1_terrain)
			elif not MapData.has_transition(edge_terrain, adj1_terrain) and MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 2
				rect = _get_source_tile(get_transtion_tile_id(2, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain, adj2_terrain)
			elif MapData.has_transition(edge_terrain, adj1_terrain) and MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 3
				rect = _get_source_tile(get_transtion_tile_id(3, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain, adj1_terrain)
			else:
				# Row 7: edge = secondary; adj1 = primary; adj2 = primary
				rect = _get_source_tile(get_transtion_tile_id(7, section), 6)
				source_img = _get_transition_image(terrain_type, edge_terrain)
	elif has_trans_adj1 or has_trans_adj2:
		if has_trans_adj1 and adj2_terrain != adj1_terrain:
			# Row 1: edge = primary; adj1 = secondary; adj2 = primary
			rect = _get_source_tile(get_transtion_tile_id(1, section), 6)
			source_img = _get_transition_image(terrain_type, adj1_terrain)
		elif has_trans_adj2 and adj1_terrain != adj2_terrain:
			# Row 2: edge = primary; adj1 = primary; adj2 = secondary
			rect = _get_source_tile(get_transtion_tile_id(2, section), 6)
			source_img = _get_transition_image(terrain_type, adj2_terrain)
		elif has_trans_adj1 and has_trans_adj2:
			# Row 3: edge = primary; adj1, adj2 = secondary
			rect = _get_source_tile(get_transtion_tile_id(3, section), 6)
			source_img = _get_transition_image(terrain_type, adj1_terrain)
	else:
		source_img = base_image
		rect = _get_source_tile(section_column_id, 7)
	
	if rect != null and source_img != null:
		image.blit_rect_mask(source_img, source_img, Rect2(rect.x, rect.y, TILE_WIDTH, TILE_HEIGHT), dest)
	else:
	# 	print("Failed to find section tile for: Section: %s\tTerrain: %s\tEdge: %s\tAdj1: %s\tAdj2: %s" % [
	# 		section,
	# 		MapData.terrain_title[terrain_type],
	# 		MapData.terrain_title[edge_terrain],
	# 		MapData.terrain_title[adj1_terrain],
	# 		MapData.terrain_title[adj2_terrain],
	# 	])
		return false
	rect = null
	source_img = null
	
	if terrain_type == MapData.TerrainType.FOREST:
		if edge_terrain == MapData.TerrainType.FOREST:
			source_img = FeatureForestEdges
			rect = _get_source_tile(section_column_id + 1, 8, 0, TILE_WIDTH, 120)
			image.blit_rect_mask(
				source_img,
				source_img,
				Rect2(rect.x, rect.y, TILE_WIDTH, 120),
				Vector2(dest.x, dest.y - 56)
			)
			return true
	
	if rect == null or source_img == null:
		return true


func _render_center(tile_pos, dest):
	var terrain_type = MapData.get_tile(tile_pos).terrain_type
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

	if terrain_type == MapData.TerrainType.FOREST:
		image.blit_rect_mask(
			FeatureForestCenter,
			FeatureForestCenter,
			Rect2(0, 0, TILE_WIDTH, TILE_HEIGHT),
			dest
		)

var current_tile_id = 0
var tile_id_to_mask = {}

func get_or_generate_tile(tile_pos: Vector2):
	var tile_bitmask = int(MapData.get_tile_bitmask(tile_pos))
	if tile_cache.has(tile_bitmask):
		return tile_cache[tile_bitmask]
	
	var tile_id = current_tile_id
	tile_id_to_mask[tile_id] = tile_bitmask
	current_tile_id += 1

	assert(tile_id < (ROWS * COLUMNS))

	var dest = Vector2(
		(tile_id % COLUMNS) * (TILE_WIDTH + PADDING),
		floor(tile_id / COLUMNS) * (TILE_HEIGHT + PADDING)
	)
	
	# Render from the top down:
	# N, NE, NW, CENTER, SW, SE, S
	
	var s_N = _render_edge(tile_pos, dest, MapData.Section.EDGE_N)
	var s_NE = _render_edge(tile_pos, dest, MapData.Section.EDGE_NE)
	var s_NW = _render_edge(tile_pos, dest, MapData.Section.EDGE_NW)
	var s_CENTER = _render_center(tile_pos, dest)
	var s_SW = _render_edge(tile_pos, dest, MapData.Section.EDGE_SW)
	var s_SE = _render_edge(tile_pos, dest, MapData.Section.EDGE_SE)
	var s_S = _render_edge(tile_pos, dest, MapData.Section.EDGE_S)

	if not s_N and not s_NE and not s_NW and not s_CENTER and not s_SW and not s_SE and not s_S:
		print("Failed to generate tile %s" % str(tile_pos))

	tile_cache[tile_bitmask] = tile_id
	return tile_id