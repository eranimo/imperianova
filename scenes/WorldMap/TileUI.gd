# Script to attach to a node which represents a hex grid
extends Node2D

onready var highlight = get_node("Highlight")

func update_highlight_pos(world_pos: Vector2) -> void:
	if highlight != null:
		highlight.position = world_pos
