extends Node
class_name TileLocation

var component_name = 'TileLocation'

var location = ReactiveState.new()

func _init(location_):
	location = ReactiveState.new(location_)
	
func _ready():
	EntitySystem.register_component(self)

func from_dict(exported):
	location = exported['loction']

func to_dict():
	return {
		"location": location.value
	}
