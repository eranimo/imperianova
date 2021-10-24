# Script to attach to a node which represents a hex grid
extends Node2D

var selection_rect = null

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
