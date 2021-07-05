extends Entity
class_name Pop

var entity_name = 'Pop'

var size = 0

func _init(location: Vector2):
	self.add_component(TileLocation.new(location))

func to_dict():
	return {
		"size": size,
	}
