extends KinematicBody2D

const entity_name = 'Unit'


export(bool) var is_selected = false setget set_selected

func setup(tile_pos):
	get_node("TilePosition").set_tile_pos(tile_pos, false)

func set_selected(value):
	is_selected = value

	if is_selected:
		$Icon.material.set_shader_param("outline_width", 20)
	else:
		$Icon.material.set_shader_param("outline_width", 0)
	
	get_node("Movement").path_visible = is_selected
	update()
