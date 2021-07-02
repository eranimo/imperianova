extends HexMap

var tile_colors = {}

var temperature_gradient = preload("res://resources/colormaps/temperature.tres")
var rainfall_gradient = preload("res://resources/colormaps/rainfall.tres")

func get_gradient_colors(gradient, step):
	var colors = {}
	for i in range(0, 100, step):
		colors[i] = gradient.interpolate(float(i) / 100)
	return colors


var temperature_colors = get_gradient_colors(temperature_gradient.gradient, 5)
var rainfall_colors = get_gradient_colors(rainfall_gradient.gradient, 5)

onready var HexShape = preload("res://assets/textures/hex-shape.png")

func _ready():
	MapManager.current_map_mode.subscribe(self, '_map_mode_change')

func _exit_tree():
	MapManager.current_map_mode.unsubscribe(self)

func _map_mode_change(map_mode):
	if map_mode == MapManager.MapMode.NONE:
		tile_colors = {}
	else:
		for pos in MapData.tiles():
			var color = Color(0, 0, 0)
			var tile_data = MapData.get_tile(pos)
			if map_mode == MapManager.MapMode.HEIGHT:
				var height = tile_data.height
				color = Color(height / 255.0, height / 255.0, height / 255.0)
			elif map_mode == MapManager.MapMode.TEMPERATURE:
				var temperature = tile_data.temperature
				color = temperature_colors.get(int(stepify(temperature, 0.05) * 100), Color(0, 0, 0))
			elif map_mode == MapManager.MapMode.RAINFALL:
				var rainfall = tile_data.rainfall
				color = rainfall_colors.get(int(stepify(rainfall, 0.05) * 100), Color(0, 0, 0))
			
			tile_colors[pos] = color
	render()

func render():
	update()

func _draw():
	for pos in MapData.tiles():
		var center = map_to_world(pos)
		if tile_colors.has(pos):
			var color = tile_colors[pos]
			draw_texture(HexShape, center, color)
