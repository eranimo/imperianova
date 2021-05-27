extends Node

var Heightmap = preload("res://scripts/Heightmap.gd")

export(int) var map_seed
export(int) var map_width
export(int) var map_height

export(Dictionary) var world_data = {}

signal map_generated

func _ready():
	SaveSystem.connect("load_complete", self, "render_map")
	render_map()

func render_map():
	for pos in world_data:
		var tile = world_data[pos]
		$MapViewport.set_tile(pos, tile.tile_id)

func generate(options):
	map_seed = options.get('map_seed')
	var size = options.get('size')
	map_width = size * 2
	map_height = size
	var heightmap = Heightmap.new(map_width, map_height)
	heightmap.generate(map_seed)
	
	for x in range(map_width):
		for y in range(map_height):
			var pos = Vector2(x, y)
			var height = heightmap.get_cell(pos)
			_create_tile(pos, height)
	render_map()
	emit_signal("map_generated")

func _create_tile(pos: Vector2, height: float):
	var tile_id
	if height < 0.5:
		tile_id = 0
	else:
		tile_id = 1
	world_data[pos] = {
		"tile_id": tile_id,
	}
	

func save():
	return {
		"world_data": world_data
	}

func load(dict):
	world_data = dict["world_data"]
