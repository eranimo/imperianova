extends HexMap

var chunk_position: Vector2 setget _set_chunk_position
var should_render = true

func _set_chunk_position(new_chunk_position):
	chunk_position = new_chunk_position

	$VisibilityNotifier2D.rect.size = Vector2(
		MapData.HEX_SIZE.x * MapData.CHUNK_SIZE.x,
		MapData.HEX_SIZE.y * MapData.CHUNK_SIZE.y
	)

func render():
	print("Render chunk ", chunk_position)

	var first_x = chunk_position.x * MapData.CHUNK_SIZE.x
	var first_y = chunk_position.y * MapData.CHUNK_SIZE.y
	for x in range(0, MapData.CHUNK_SIZE.x):
		for y in range(0, MapData.CHUNK_SIZE.y):
			var map_tile = Vector2(first_x + x, first_y + y)
			var local_tile = Vector2(x, y)
			$Terrain.set_tile(map_tile, local_tile)

	$Terrain.render()
	should_render = false

func _on_entered(viewport):
	visible = true

	if should_render:
		render()

func _on_exited(viewport):
	visible = false
