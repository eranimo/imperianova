extends Node

var WorldNoise = preload("res://scripts/WorldNoise.gd")

# State
export(int) var map_seed
export(int) var map_width
export(int) var map_height
export(Dictionary) var map_options
export(Dictionary) var world_data = {}

signal map_generated

func _ready():
	SaveSystem.connect("load_complete", self, "render_map")
	if SaveSystem.pending_save:
		return

func _exit_tree():
	SaveSystem.disconnect("load_complete", self, "render_map")

func render_map():
	print("Render map ", world_data.size())
	MapData.set_world_data(self)
	MapManager.connect_world(self)
	
	var time_start = OS.get_ticks_msec()
	$MapViewport/Viewport/WorldMap.render()
	print("World map rendering: ", OS.get_ticks_msec() - time_start)

func generate(options):
	print("World generate")
	MapData.reset_map()
	map_seed = options.get('map_seed')
	var size = options.get('size')
	var sealevel = options.get('sealevel')
	map_options = options
	map_width = size * 2
	map_height = size

	var heightmap = WorldNoise.new(map_width, map_height)
	heightmap.generate(map_seed)
	
	var temperature_map = WorldNoise.new(map_width, map_height)
	temperature_map.octaves = 3
	temperature_map.period = 0.5
	temperature_map.generate(map_seed * 2)

	var rainfall_map = WorldNoise.new(map_width, map_height)
	rainfall_map.octaves = 4
	rainfall_map.period = 0.35
	rainfall_map.generate(map_seed * 3)
	
	for x in range(map_width):
		for y in range(map_height):
			var pos = Vector2(x, y)
			var height = int(heightmap.get_cell(pos) * 255)
			var temperature = temperature_map.get_cell(pos)
			var rainfall = rainfall_map.get_cell(pos)
			var terrain_type
			if height < sealevel:
				terrain_type = MapData.TerrainType.OCEAN
			else:
				if temperature < 0.60:
					if rainfall < 0.5:
						terrain_type = MapData.TerrainType.GRASSLAND
					else:
						terrain_type = MapData.TerrainType.FOREST
				else:
					terrain_type = MapData.TerrainType.DESERT
					
			world_data[pos] = {
				"height": height,
				"terrain_type": terrain_type,
			}
	render_map()
	emit_signal("map_generated")

func to_dict():
	return {
		"map_options": map_options,
		"world_data": world_data,
		"map_width": map_width,
		"map_height": map_height,
	}

func from_dict(dict):
	map_options = dict["map_options"]
	world_data = dict["world_data"]
	map_width = dict["map_width"]
	map_height = dict["map_height"]
