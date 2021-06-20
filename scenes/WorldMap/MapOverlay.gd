extends HexMap

var tile_colors = {}

onready var HexShape = preload("res://assets/textures/overlay.tres")

func _ready():
	MapManager.current_map_mode.subscribe(self, '_map_mode_change')

func _exit_tree():
	MapManager.current_map_mode.unsubscribe(self)

func _map_mode_change(map_mode):
	if map_mode == MapManager.MapMode.NONE:
		tile_colors = {}
	else:
		for pos in MapData.tiles:
			var color = Color(0, 0, 0)

			if map_mode == MapManager.MapMode.HEIGHT:
				var height = MapData.world_data[pos].height
				color = Color(height / 255.0, height / 255.0, height / 255.0)
			
			tile_colors[pos] = color
	render()

func render():
	update()

func _draw():
	for pos in MapData.tiles:
		var center = map_to_world(pos)
		if tile_colors.has(pos):
			var color = tile_colors[pos]
			draw_texture(HexShape, center, color)
