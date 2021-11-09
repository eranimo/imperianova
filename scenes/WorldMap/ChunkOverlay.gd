# DEPRECATED
extends Node2D


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
	var sealevel = float(WorldData.sealevel)
	for chunk_tile in get_parent().chunk_tiles:
		var pos = chunk_tile.global
		var color = Color(0, 0, 0)
		var tile_data = WorldData.GetTile(pos.x, pos.y)
		if map_mode == MapManager.MapMode.TERRAIN:
			var terrainType =  tile_data.terrain_type
			color = MapData.terrain_colors[terrainType]
		elif map_mode == MapManager.MapMode.HEIGHT:
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
		
		$HexMeshGrid.set_hex_color(chunk_tile.local, color)

func _map_mode_change(_map_mode):
	update_colors()

func render():
	$HexMeshGrid.setup_grid()
	update_colors()
