extends Node
class_name System

var entities = []

signal entity_added(entity)
signal entity_removed(entity)

export(int) var tick_interval = 1

func _ready():
	EntitySystem.register_system(self)

func update(_tick: int):
	pass

func entity_filter(_entity):
	return true

func get_entities():
	return entities

func size():
	return entities.size()

func entity_added(_entity):
	pass

func entity_removed(_entity):
	pass

func _add_entity(entity):
	entities.append(entity)
	entity_added(entity)
	emit_signal("entity_added", entity)

func _remove_entity(entity):
	entities.erase(entity)
	entity_removed(entity)
	emit_signal("entity_removed", entity)
