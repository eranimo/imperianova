extends HexMap

var tile_colors = {}

var height_gradient = preload("res://resources/colormaps/mapmode-height.tres")
var temperature_gradient = preload("res://resources/colormaps/mapmode-temperature.tres")
var rainfall_gradient = preload("res://resources/colormaps/mapmode-rainfall.tres")

func get_gradient_colors(gradient, step):
	var colors = {}
	for i in range(0, 100, step):
		colors[i] = gradient.interpolate(float(i) / 100)
	return colors

var height_colors = get_gradient_colors(height_gradient, 1)
var temperature_colors = get_gradient_colors(temperature_gradient, 5)
var rainfall_colors = get_gradient_colors(rainfall_gradient, 5)

onready var HexShape = preload("res://assets/textures/hex-shape.png")

func _ready():
	MapManager.current_map_mode.subscribe(self, '_map_mode_change')

func _exit_tree():
	MapManager.current_map_mode.unsubscribe(self)

func update_colors():
	var map_mode = MapManager.current_map_mode.value
	if map_mode == MapManager.MapMode.NONE:
		tile_colors = {}
		return
	var sealevel = float(MapData.game_world.map_options.sealevel)
	for chunk_tile in get_parent().chunk_tiles:
		var pos = chunk_tile.global
		var color = Color(0, 0, 0)
		var tile_data = MapData.get_tile(pos)
		if map_mode == MapManager.MapMode.HEIGHT:
			var height = tile_data.height
			var ratio
			if height < sealevel:
				ratio = ((sealevel - height) / sealevel) / 2
			else:
				ratio = 0.5 + (((height - sealevel) / (float(255) - sealevel)) / 2)
			color = height_colors.get(int(stepify(ratio, 0.05) * 100), Color(0, 0, 0))
		elif map_mode == MapManager.MapMode.TEMPERATURE:
			var temperature = tile_data.temperature
			color = temperature_colors.get(int(stepify(temperature, 0.05) * 100), Color(0, 0, 0))
		elif map_mode == MapManager.MapMode.RAINFALL:
			var rainfall = tile_data.rainfall
			color = rainfall_colors.get(int(stepify(rainfall, 0.05) * 100), Color(0, 0, 0))
		
		tile_colors[pos] = color

func _map_mode_change(_map_mode):
	update_colors()
	render()

func render():
	update_colors()
	update()

func _draw():
	for chunk_tile in get_parent().chunk_tiles:
		if tile_colors.has(chunk_tile.global):
			var color = tile_colors[chunk_tile.global]
			var center = map_to_world(chunk_tile.local)
			draw_texture(HexShape, center, color)
