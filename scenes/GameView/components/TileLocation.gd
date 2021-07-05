extends Component
class_name TileLocation

var component_name = 'TileLocation'

var location = EntityValue.new()

func _init(location_):
	location = EntityValue.new(location_)

func from_dict(exported):
	location = exported['loction']

func to_dict():
	return {
		"location": location.value
	}
