extends Node
class_name EntitySystem

var _current_id = 0
var _entity_by_id = {}
var systems = []
var _system_ticks_remaining = {}

func _ready():
	for child in get_children():
		register_system(child)

func add_entity(entity: Entity, id = null):
	if id == null:
		entity.id = _current_id
		_current_id += 1
	
	_entity_by_id[entity.id] = entity
		
	for system in systems:
		if system.entity_filter(entity):
			system.entities.append(entity)
	
	return entity

func get_entity(entity_id: int):
	return _entity_by_id.get(entity_id)

func register_system(system):
	print("Added system ", system.name)
	if system.has_method("init"):
		system.call("init", self)
	_system_ticks_remaining[system] = system.tick_interval
	systems.append(system)

func update(tick: int):
	for system in systems:
		if _system_ticks_remaining[system] == 1:
			system.update(tick)
			_system_ticks_remaining[system] = system.tick_interval
		else:
			_system_ticks_remaining[system] -= 1

func to_dict():
	var entities = []
	for entity_id in _entity_by_id:
		var entity = _entity_by_id[entity_id]
		entities.append({
			"id": entity_id,
			"type": entity.entity_type,
			"data": entity.to_dict(),
		})
	return entities

func from_dict(entities):
	for e in entities:
		add_entity(e)
