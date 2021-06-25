extends Node
class_name System

var entities = []

export(int) var tick_interval = 1

func update(_tick: int):
	pass

func entity_filter(_entity):
	return true
