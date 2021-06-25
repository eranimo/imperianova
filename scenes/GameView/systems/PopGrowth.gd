extends System


func entity_filter(entity):
	return entity.entity_type == 'Pop'

func update(ticks):
	print("(%s) Entities in PopGrowth: %s" % [ticks, entities.size()])
	for entity in entities:
		entity.size += 1
