extends Node

var world
var tile_neighbors = {}

const CHUNK_SIZE = Vector2(10, 10)
const HEX_SIZE = Vector2(64, 60)

# Terrain type
enum TerrainType {
	OCEAN,
	GRASSLAND,
	DESERT,
	FOREST,
}

const TerrainTypeCount = 4

var terrain_title = {
	TerrainType.OCEAN: "Ocean",
	TerrainType.GRASSLAND: "Grassland",
	TerrainType.DESERT: "Desert",
	TerrainType.FOREST: "Forest",
}

const terrain_transitions = {
	TerrainType.OCEAN: [TerrainType.GRASSLAND, TerrainType.DESERT, TerrainType.FOREST],
	TerrainType.GRASSLAND: [TerrainType.DESERT, TerrainType.FOREST],
	TerrainType.FOREST: [TerrainType.DESERT],
}

# Feature type
enum FeatureType {
	NONE,
	FOREST,
	# ROAD?
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
  Direction.SE: [Direction.S, Direction.NE],
  Direction.NE: [Direction.SE, Direction.N],
  Direction.N: [Direction.NE, Direction.NW],
  Direction.NW: [Direction.N, Direction.SW],
  Direction.SW: [Direction.NW, Direction.S],
  Direction.S: [Direction.SW, Direction.SE],
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
  Direction.SE: Direction.S,
  Direction.NE: Direction.SE,
  Direction.N: Direction.NE,
  Direction.NW: Direction.N,
  Direction.SW: Direction.NW,
  Direction.S: Direction.SW,
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

func set_world_data(_world):
	world = _world

	for pos in world.world_data:
		tile_neighbors[pos] = {
			Direction.SE: _get_neighbor(pos, Direction.SE),
			Direction.S: _get_neighbor(pos, Direction.S),
			Direction.SW: _get_neighbor(pos, Direction.SW),
			Direction.NW: _get_neighbor(pos, Direction.NW),
			Direction.N: _get_neighbor(pos, Direction.N),
			Direction.NE: _get_neighbor(pos, Direction.NE),
		}

func reset_map():
	world = null

func is_valid_pos(pos: Vector2):
	if pos.x < 0 or pos.y < 0:
		return false
	if pos.x >= world.map_width or pos.y >= world.map_height:
		return false
	return true

func get_neighbor(pos: Vector2, dir):
	return tile_neighbors[pos][dir]

func _get_neighbor(pos: Vector2, dir):
	var parity = int(pos.x) & 1
	var dir_add = oddq_directions[parity][dir]
	var n_pos = Vector2(
		pos.x + dir_add[0],
		pos.y + dir_add[1]
	)
	
	# wrap around the world
	if n_pos.x < 0:
		n_pos.x = world.map_width - 1
	if n_pos.y < 0:
		n_pos.y = 0
		n_pos.x = int((world.map_width-1) - ((n_pos.x / ((world.map_width-1) / 2)) * ((world.map_width-1) / 2)))
	if n_pos.x >= world.map_width:
		n_pos.x = 0
	if n_pos.y >= world.map_height:
		n_pos.x = int((world.map_width-1) - ((n_pos.x / ((world.map_width-1) / 2)) * ((world.map_width-1) / 2)))
		n_pos.y = world.map_height - 1

	return n_pos

func tiles():
	return world.world_data

func get_tile(pos: Vector2):
	return world.world_data[pos]

func get_neighbor_tile(pos: Vector2, dir):
	return world.world_data[get_neighbor(pos, dir)]

func has_transition(terrain1, terrain2):
	if not terrain_transitions.has(terrain1):
		return false
	return terrain_transitions.get(terrain1).has(terrain2)
