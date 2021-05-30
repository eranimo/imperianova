extends Node

var world_data = {} 
var map_width
var map_height
var tiles = {}


# Terrain type
enum TerrainType {
	OCEAN,
	GRASSLAND
}

const TerrainTypeCount = 2

var terrain_title = {
	TerrainType.OCEAN: "Ocean",
	TerrainType.GRASSLAND: "Grassland",
}

const terrain_transitions = {
	TerrainType.OCEAN: [TerrainType.GRASSLAND]
}


# Feature type
enum FeatureType {
	NONE,
	FOREST
}

const FeatureTypeCount = 2

var feature_title = {
	FeatureType.NONE: "None",
	FeatureType.FOREST: "Forest",
}


# Direction
enum Direction {
	SE
	NE
	N
	NW
	SW
	S
}

const oddq_directions = [
  [
	[+1, 0], [+1, -1], [0, -1],
	[-1, -1], [-1, 0], [0, +1]
  ],
  [
	[+1, +1], [+1, 0], [0, -1],
	[-1, 0], [-1, +1], [0, +1]
  ],
];

const adjacent_directions = {
  Direction.SE: [Direction.NE, Direction.S],
  Direction.NE: [Direction.N, Direction.SE],
  Direction.N: [Direction.NW, Direction.NE],
  Direction.NW: [Direction.SW, Direction.N],
  Direction.SW: [Direction.S, Direction.NW],
  Direction.S: [Direction.SE, Direction.SW],
};

const opposite_directions = {
  Direction.SE: Direction.NW,
  Direction.NE: Direction.SW,
  Direction.N: Direction.S,
  Direction.NW: Direction.SE,
  Direction.SW: Direction.NE,
  Direction.S: Direction.N,
}

const direction_clockwise = {
  [Direction.SE]: Direction.S,
  [Direction.NE]: Direction.SE,
  [Direction.N]: Direction.NE,
  [Direction.NW]: Direction.N,
  [Direction.SW]: Direction.NW,
  [Direction.S]: Direction.SW,
};

const direction_counter_clockwise = {
  Direction.SE: Direction.NE,
  Direction.NE: Direction.N,
  Direction.N: Direction.NW,
  Direction.NW: Direction.SW,
  Direction.SW: Direction.S,
  Direction.S: Direction.SE,
};

enum Section {
	CENTER,
	EDGE_SE,
	EDGE_S,
	EDGE_SW,
	EDGE_NW,
	EDGE_N,
	EDGE_NE,
}

const section_to_direction = {
	Section.EDGE_SE: Direction.SE,
	Section.EDGE_S: Direction.S,
	Section.EDGE_SW: Direction.SW,
	Section.EDGE_NW: Direction.NW,
	Section.EDGE_N: Direction.N,
	Section.EDGE_NE: Direction.NE,
}

const direction_to_section = {
	Direction.SE: Section.EDGE_SE,
	Direction.S: Section.EDGE_S,
	Direction.SW: Section.EDGE_SW,
	Direction.NW: Section.EDGE_NW,
	Direction.N: Section.EDGE_N,
	Direction.NE: Section.EDGE_NE,
}

func reset_map():
	world_data = {}
	tiles = {}

func is_valid_pos(pos: Vector2):
	if pos.x < 0 or pos.y < 0:
		return false
	if pos.x >= map_width or pos.y >= map_height:
		return false
	return true

func get_neighbor(pos: Vector2, dir):
	var parity = int(pos.x) & 1
	var dir_add = oddq_directions[parity][dir]
	var n_pos = Vector2(
		pos.x + dir_add[0],
		pos.y + dir_add[1]
	)
	
	# wrap around the world
	if n_pos.x < 0:
		n_pos.x = map_width - 1
	if n_pos.y < 0:
		n_pos.y = 0
		n_pos.x = int((map_width-1) - ((n_pos.x / ((map_width-1) / 2)) * ((map_width-1) / 2)))
	if n_pos.x >= map_width:
		n_pos.x = 0
	if n_pos.y >= map_height:
		n_pos.x = int((map_width-1) - ((n_pos.x / ((map_width-1) / 2)) * ((map_width-1) / 2)))
		n_pos.y = map_height - 1

	return n_pos

func get_terrain_type(tile_pos):
	return world_data[tile_pos].terrain_type

func set_world_data(_world_data, _map_width, _map_height):
	world_data = _world_data
	map_width = _map_width
	map_height = _map_height
	
	for pos in world_data:
		var terrain_type = get_terrain_type(pos)
		tiles[pos] = {
			"terrain_type": terrain_type,
			"edge": {
				Direction.SE: get_terrain_type(get_neighbor(pos, Direction.SE)),
				Direction.S: get_terrain_type(get_neighbor(pos, Direction.S)),
				Direction.SW: get_terrain_type(get_neighbor(pos, Direction.SW)),
				Direction.NW: get_terrain_type(get_neighbor(pos, Direction.NW)),
				Direction.N: get_terrain_type(get_neighbor(pos, Direction.N)),
				Direction.NE: get_terrain_type(get_neighbor(pos, Direction.NE)),
			},
		}

func get_tile_bitmask(tile_pos: Vector2):
	var terrain_type = tiles[tile_pos].terrain_type
	var terrain_SE = tiles[tile_pos].edge[Direction.SE]
	var terrain_S = tiles[tile_pos].edge[Direction.S]
	var terrain_SW = tiles[tile_pos].edge[Direction.SW]
	var terrain_NW = tiles[tile_pos].edge[Direction.NW]
	var terrain_N = tiles[tile_pos].edge[Direction.N]
	var terrain_NE = tiles[tile_pos].edge[Direction.NE]
	
	# TODO: remove duplicates of non-transitioning terrain combinations
	
	return (
		pow(TerrainTypeCount, 0) * terrain_type
		+ pow(TerrainTypeCount, 1) * terrain_SE
		+ pow(TerrainTypeCount, 2) * terrain_S
		+ pow(TerrainTypeCount, 3) * terrain_SW
		+ pow(TerrainTypeCount, 4) * terrain_NW
		+ pow(TerrainTypeCount, 5) * terrain_N
		+ pow(TerrainTypeCount, 6) * terrain_NE
	)

func get_tile_section_bitmask(tile_pos: Vector2, section):
	var center_terrain_type = tiles[tile_pos].terrain_type
	
	if section == Section.CENTER:
		return (
			pow(TerrainTypeCount - 1, 0) * center_terrain_type
		)
	else:
		var edge_terrain_type = tiles[tile_pos].edge[section_to_direction[section]]
		return (
			pow(TerrainTypeCount - 1, 0) * center_terrain_type
			+ pow(TerrainTypeCount - 1, 1) * edge_terrain_type
		)
