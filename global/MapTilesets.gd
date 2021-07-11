extends Node

const TILE_WIDTH = 64
const TILE_HEIGHT = 60
const TILE_SIZE = Vector2(TILE_WIDTH, TILE_HEIGHT)

var missing_tilesets = []

var OceanBase = preload("res://assets/textures/tilesets/Ocean.png")
var GrasslandBase = preload("res://assets/textures/tilesets/Grass.png")
var DesertBase = preload("res://assets/textures/tilesets/Desert.png")
var ForestBase = preload("res://assets/textures/tilesets/Forest.png")

var OceanGrasslandTrans2 = preload("res://assets/textures/tilesets/Ocean-Grass.png")
var OceanDesertTrans2 = preload("res://assets/textures/tilesets/Ocean-Desert.png")
var OceanForestTrans2 = preload("res://assets/textures/tilesets/Ocean-Forest.png")
var GrasslandDesertTrans2 = preload("res://assets/textures/tilesets/Grass-Desert.png")
var GrasslandForestTrans2 = preload("res://assets/textures/tilesets/Grass-Forest.png")
var ForestDesertTran2 = preload("res://assets/textures/tilesets/ForestGrass-Desert.png")

var OceanGrasslandDesertTrans3 = preload("res://assets/textures/tilesets/OceanGrass-Desert.png")
var GrasslandForestDesertTrans3 = preload("res://assets/textures/tilesets/Grassland-Forest(Desert).png")
var OceanGrasslandForestTrans3 = preload("res://assets/textures/tilesets/Ocean-Grassland(Forest).png")
var OceanForestDesertTrans3 = preload("res://assets/textures/tilesets/Ocean-Forest(Desert).png")

# features
var FeatureForestCenter = preload("res://assets/textures/tilesets/Feature-Forest-Center.png")
var FeatureForestEdges = preload("res://assets/textures/tilesets/Trees-Hex.png")

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

func get_tile_rect(id, columns, tile_width=TILE_WIDTH, tile_height=TILE_HEIGHT, padding=0) -> Rect2:
	return Rect2(
		Vector2(
			(id % columns) * (tile_width + padding),
			floor(id / columns) * (tile_height + padding)
		),
		Vector2(tile_width + padding, tile_height + padding)
	)

func get_transtion_tile_id(row, section):
	var section_column_id = transition_column_ids[section]
	return ((row - 1) * 6) + section_column_id

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