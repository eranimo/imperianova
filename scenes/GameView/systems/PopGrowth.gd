extends System

var pop_locations = MapSet.new()
var pop_location_total = MapSet.new()

func entity_filter(entity):
	return entity.entity_type == 'Pop'

func entity_added(entity):
	pop_locations.add(entity.location.value, entity)
	entity.location.subscribe(self, "_pop_moved", entity)

func entity_removed(entity):
	pop_locations.remove(entity.location.value, entity)
	entity.location.unsubscribe(self, "_pop_moved")

func _pop_moved(new_location, old_location, pop):
	pop_locations.remove(old_location, pop)
	pop_locations.add(new_location, pop)

func update(ticks):
	for location in pop_locations.keys():
		for entity in pop_locations.get(location):
			entity.size += 1
	print("(%s) Entities in PopGrowth: %s" % [ticks, size()])
