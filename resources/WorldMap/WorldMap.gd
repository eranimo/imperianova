extends Node
class_name WorldMap

var Heightmap = preload("res://resources/WorldMap/Heightmap.gd")

export(int) var map_seed
export(int) var heightmap
export(int) var map_width
export(int) var map_height

func _init(size, map_seed_):
	map_seed = map_seed_
	map_width = size * 2
	map_height = size
	heightmap = Heightmap.new(map_width, map_height)
	heightmap.generate(map_seed)

func get_cell(pos: Vector2):
	return heightmap.get_cell(pos)
	
