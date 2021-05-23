extends Node2D

func _ready():
	var map = get_node("Map")
	map.call("setup_map", 150, 75)

