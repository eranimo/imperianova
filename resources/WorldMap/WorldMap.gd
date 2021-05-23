extends Node
class_name WorldMap

var Heightmap = preload("./Heightmap.gd")

var map_seed = 1234
var heightmap
var map_width
var map_height

func _init(size):
	map_width = size * 2
	map_height = size
	heightmap = Heightmap.new(map_width, map_height)
	heightmap.generate(map_seed)

func get_cell(pos: Vector2):
	return heightmap.get_cell(pos)
	
