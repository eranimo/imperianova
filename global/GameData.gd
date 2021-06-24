extends Node

var type_objects_by_name = {}
var type_objects_by_id = {}

var tile_developments = load_type_object('tile_developments', 'tile_developments.json')


class TypeObjectRef:
	var name
	var id
	var data

	func _init(name_, id_, data_):
		name = name_
		id = id_
		data = data_
	
	func to_dict():
		return {
			"name": name,
			"id": id,
		}

func load_type_object(name, filename):
	type_objects_by_name[name] = []
	type_objects_by_id[name] = {}
	var file = File.new()
	file.open('res://data/%s' % filename, file.READ)
	var json = file.get_as_text()
	var json_result = JSON.parse(json)
	assert(not json_result.error, "Failed to load type object named '%s'" % name)
	file.close()

	var items = json_result.result
	for item in items:
		var ref = TypeObjectRef.new(name, item['id'], item)
		type_objects_by_id[name][item['id']] = ref
		type_objects_by_name[name].append(ref)
	
	return items

func get_by_id(name, id):
	return type_objects_by_id[name].get(id)

func get_all(name):
	assert(type_objects_by_id.has(name), "Type object named '%s' does not exist" % name)
	return type_objects_by_id.get(name)
