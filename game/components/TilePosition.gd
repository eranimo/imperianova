extends Node
class_name TilePosition

const component_name = 'TilePosition'

var tile_pos setget set_tile_pos

onready var tween = $Tween

func _ready():
	EntitySystem.register_component(self)

func set_tile_pos(value: Vector2, should_tween = true):
	tile_pos = value
	var parent = get_parent()
	var new_position = MapManager.world_map.map_to_world(tile_pos)
	if should_tween:
		tween.interpolate_property(
			parent, "position",
			parent.position,
			new_position,
			EntitySystem.CURRENT_TICKS_PER_DAY / 60.0,
			Tween.TRANS_LINEAR, Tween.EASE_IN_OUT
		)
		tween.start()
	else:
		parent.position = new_position
