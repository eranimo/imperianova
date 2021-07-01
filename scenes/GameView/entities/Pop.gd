extends Entity
class_name Pop

const entity_type = 'Pop'

var size = 0
var location = EntityValue.new()

func to_dict():
	return {
		"size": size,
		"location": location.value,
	}
