extends Object
class_name Entity

var id: int
var components: Dictionary = {}

func to_dict():
	assert(false, "to_dict() must be implemented")

func from_dict(_exported: Dictionary):
	assert(false, "from_dict() must be implemented")

func get_component(component_name: String):
	assert(components.has(component_name), "Entity '%s' does not have component '%s'" % [self.entity_name, component_name])
	return components[component_name]

func add_component(component: Component):
	component.entity = self
	components[component.component_name] = component

func has_component(component_name: String):
	return components.has(component_name)

func remove_component(component: Component):
	component.entity = null
	return components.erase(component)
