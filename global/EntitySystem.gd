extends Node

var _entities_components = {}
var _system_ticks_remaining = {}
var systems = []
var _entities_to_delete = []
var _components_entities = {}

var CURRENT_TICKS_PER_DAY


func setup():
	_entities_components = {}

func is_entity(entity):
	return entity != null and 'entity_name' in entity

func is_component(component):
	return component != null and 'component_name' in component

func register_entity(entity):
	assert(is_entity(entity), "Entity is required")
	if not _entities_components.has(entity):
		_entities_components[entity] = {}

func register_component(component: Node):
	assert(is_component(component), "Component is required")
	var entity = component.get_parent()
	register_entity(entity)
	_entities_components[entity][component.component_name] = component
	_components_entities[component] = entity

	component.connect("tree_exiting", self, "unregister_component")

func unregister_component(component: Node):
	var entity = component.get_parent()
	_components_entities.clear(component)
	_entities_components[entity].clear()

func remove_entity(entity):
	assert(is_entity(entity), "Entity is required")
	entity.queue_free()
	_entities_components.clear(entity)

func get_components(entity):
	assert(is_entity(entity), "Entity is required")
	return _entities_components.get(entity)

func get_component(sibling_component: Node, component_name: String):
	var entity = sibling_component.get_parent()
	assert(is_entity(entity), "Entity is required")
	assert(_entities_components[entity].has(component_name), "Entity '%s' has no component '%s'" % [entity.entity_name, component_name])
	return _entities_components[entity][component_name]

func register_system(system):
	print("Added system ", system.name)
	if system.has_method("setup"):
		system.call("setup", self)
	_system_ticks_remaining[system] = system.tick_interval
	systems.append(system)

func emit_event(sibling_component: Node, event_name: String, event_data: Dictionary):
	var entity = sibling_component.get_parent()
	assert(is_entity(entity), "Entity is required")
	for component_name in _entities_components[entity]:
		var component = _entities_components[entity][component_name]
		if component.has_method("handle_event"):
			component.callv("handle_event", [event_name, event_data])

func update(tick: int):
	for entity in _entities_to_delete:
		entity.free()

	for entity in _entities_components:
		for component_name in _entities_components[entity]:
			var component = _entities_components[entity][component_name]
			if component.has_method("game_process"):
				component.call("game_process")

	for system in systems:
		if _system_ticks_remaining[system] == 1:
			system.update(tick)
			_system_ticks_remaining[system] = system.tick_interval
		else:
			_system_ticks_remaining[system] -= 1
