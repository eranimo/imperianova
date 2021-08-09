# Script to attach to a node which represents a hex grid
extends Node2D

onready var highlight = get_node("Highlight")

var selection_rect = null

func update_highlight_pos(world_pos: Vector2) -> void:
	if highlight != null:
		highlight.position = world_pos

func update_selection_rect(rect):
	selection_rect = rect
	update()

func _draw():
	if selection_rect:
		draw_rect(
			selection_rect,
			Color(1, 1, 1),
			false
		)
