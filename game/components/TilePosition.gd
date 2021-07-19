extends Node
class_name TilePosition

const component_name = 'TilePosition'

var tile_pos setget set_tile_pos

func _ready():
	EntitySystem.register_component(self)

func set_tile_pos(value: Vector2):
	tile_pos = value
	get_parent().position = MapManager.world_map.map_to_world(tile_pos)
