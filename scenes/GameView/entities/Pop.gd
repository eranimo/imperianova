extends Entity
class_name Pop

const entity_type = 'Pop'

var size: int = 0
var location: Vector2

func to_dict():
	return {
		"size": size,
		"location": location,
	}
