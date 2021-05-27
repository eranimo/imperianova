extends Node

func generate():
	var size = 100
	print("Generating world ", (size * (size * 2)))
	$World.generate({
		"map_seed": rand_range(0, 100),
		"size": size,
	})

