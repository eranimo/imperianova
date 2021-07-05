extends Node

var _current_id = 0
var _entity_by_id = {}
var systems = []
var _system_ticks_remaining = {}
var _entities_to_delete = []


func setup():
	for entity_id in _entity_by_id:
		_entity_by_id[entity_id].quue_free()
	systems = []
	_system_ticks_remaining = {}
	_entities_to_delete = []
	_current_id = 0

func add_entity(entity, id = null):
	if id == null:
		entity.id = _current_id
		_current_id += 1
	
	_entity_by_id[entity.id] = entity
		
	for system in systems:
		if system.entity_filter(entity):
			system._add_entity(entity)
	
	return entity

func remove_entity(entity):
	_entity_by_id.clear(entity.id)
	for system in systems:
		if system.entity_filter(entity) and system.entities.has(entity):
			system._remove_entity(entity)

func delete_entity(entity):
	_entities_to_delete.append(entity)

func get_entity(entity_id: int):
	return _entity_by_id.get(entity_id)

func register_system(system):
	print("Added system ", system.name)
	if system.has_method("setup"):
		system.call("setup", self)
	_system_ticks_remaining[system] = system.tick_interval
	systems.append(system)

func update(tick: int):
	for entity in _entities_to_delete:
		entity.free()

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
			"type": entity.name,
			"data": entity.to_dict(),
		})
	return entities

func from_dict(entities):
	for e in entities:
		add_entity(e)
