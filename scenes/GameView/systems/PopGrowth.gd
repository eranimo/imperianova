extends System

var pop_locations = MapSet.new()
var pop_location_total = MapSet.new()

func entity_filter(entity):
	return entity.entity_name == 'Pop'

func entity_added(entity):
	var tile_location = entity.get_component('TileLocation')
	pop_locations.add(tile_location.location.value, entity)
	tile_location.location.subscribe(self, "_pop_moved", entity)

func entity_removed(entity):
	var tile_location = entity.get_component('TileLocation')
	pop_locations.remove(tile_location.location.value, entity)
	tile_location.location.unsubscribe(self, "_pop_moved")

func _pop_moved(new_location, old_location, pop):
	pop_locations.remove(old_location, pop)
	pop_locations.add(new_location, pop)

func update(ticks):
	for location in pop_locations.keys():
		for entity in pop_locations.get(location):
			entity.size += 1
	print("(%s) Entities in PopGrowth: %s" % [ticks, size()])
