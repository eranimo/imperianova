extends ViewportContainer

func _ready():
	var map = get_node("Viewport/Map")
	map.call("setup_map", 150, 75)

