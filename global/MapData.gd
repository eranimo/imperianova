extends Node

enum TerrainType {
	OCEAN,
	GRASSLAND
}

const TerrainTypeCount = 2

var terrain_title = {
	TerrainType.OCEAN: "Ocean",
	TerrainType.GRASSLAND: "Grassland"
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

var world_data = {} 
var map_width
var map_height
var tiles = {}

enum Direction {
	SE
	NE
	N
	NW
	SW
	S
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
	assert(is_valid_pos(tile_pos))
	var cell = world_data[tile_pos]
	return cell.terrain_type

func set_world_data(_world_data, _map_width, _map_height):
	world_data = _world_data
	map_width = _map_width
	map_height = _map_height
	
	for pos in world_data:
		var terrain_type = get_terrain_type(pos)
		tiles[pos] = {
			"terrain_type": terrain_type,
			"edge_terrain_type_SE": get_terrain_type(get_neighbor(pos, Direction.SE)),
			"edge_terrain_type_S": get_terrain_type(get_neighbor(pos, Direction.S)),
			"edge_terrain_type_SW": get_terrain_type(get_neighbor(pos, Direction.SW)),
			"edge_terrain_type_NW": get_terrain_type(get_neighbor(pos, Direction.NW)),
			"edge_terrain_type_N": get_terrain_type(get_neighbor(pos, Direction.N)),
			"edge_terrain_type_NE": get_terrain_type(get_neighbor(pos, Direction.NE)),
		}

func get_terrain_bitmask(tile_pos):
	var terrain_type = tiles[tile_pos].terrain_type
	var terrain_SE = tiles[tile_pos].edge_terrain_type_SE
	var terrain_S = tiles[tile_pos].edge_terrain_type_S
	var terrain_SW = tiles[tile_pos].edge_terrain_type_SW
	var terrain_NW = tiles[tile_pos].edge_terrain_type_NW
	var terrain_N = tiles[tile_pos].edge_terrain_type_N
	var terrain_NE = tiles[tile_pos].edge_terrain_type_NE
	
	return (
		pow(TerrainTypeCount - 1, 0) * terrain_type +
		pow(TerrainTypeCount - 1, 1) * terrain_SE +
		pow(TerrainTypeCount - 1, 2) * terrain_S +
		pow(TerrainTypeCount - 1, 3) * terrain_SW +
		pow(TerrainTypeCount - 1, 4) * terrain_NW +
		pow(TerrainTypeCount - 1, 5) * terrain_N +
		pow(TerrainTypeCount - 1, 6) * terrain_NE
	)
