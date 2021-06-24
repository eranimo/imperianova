extends Node
class_name EntitySystem

var _current_id = 0
var _entity_by_id = {}
var systems = []

func _ready():
	for child in get_children():
		register_system(child)

func new_entity() -> Entity:
	var entity = Entity.new()
	entity.id = _current_id

	_current_id += 1
	return entity

func get_entity(entity_id: int):
	return _entity_by_id.get(entity_id)

func register_system(system):
	systems.append(system)

func update(tick: int):
	for system in systems:
		system.update(tick)
