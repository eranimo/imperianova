extends Node

func _ready():
	$World.connect("map_generated", self, "render_map")

func on_game_loaded():
	render_map()

func render_map():
	for pos in $World.world_data:
		var tile = $World.world_data[pos]
		$MapViewport.set_tile(pos, tile.tile_id)

func generate():
	var size = 100
	print("Generating world ", (size * (size * 2)))
	$World.generate({
		"map_seed": rand_range(0, 100),
		"size": size,
	})

