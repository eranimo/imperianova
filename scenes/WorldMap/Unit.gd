extends KinematicBody2D

const entity_name = 'Unit'

func setup(tile_pos):
	get_node("TilePosition").set_tile_pos(tile_pos, false)
