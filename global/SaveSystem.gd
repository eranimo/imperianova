extends Node

var current_save = null

const SAVE_PATH = "user://saves"

func get_saves():
	var dir = Directory.new()
	var saves = []
	if dir.open(SAVE_PATH) == OK:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while file_name != "":
			if not dir.current_is_dir():
				saves.append(file_name.split('.')[0])
			file_name = dir.get_next()
	else:
		print("An error occurred when trying to access the path.")
	return saves

func init_save_system():
	# create save directory
	var dir = Directory.new()
	if not dir.dir_exists(SAVE_PATH):
		dir.make_dir(SAVE_PATH)

func _get_persisted_nodes():
	return get_tree().get_nodes_in_group("persist")

func _get_save_file(save_name):
	return "%s/%s" % [SAVE_PATH, save_name]

func save_game(save_name):
	var save_file = File.new()
	save_file.open(_get_save_file(save_name), File.WRITE)
	
	var objects = {}
	var nodes = _get_persisted_nodes()
	for node in nodes:
		if node.has_method("save"):
			objects[node.save_id] = node.save()
	
	save_file.store_var(objects, true)
	save_file.close()

func load_game(save_name):
	var save_file = File.new()
	var file_path = _get_save_file(save_name)

	if not save_file.file_exists(file_path):
		return
	
	var nodes = _get_persisted_nodes()
	for node in nodes:
		node.queue_free()

	save_file.open(file_path, File.READ)
	var objects = save_file.get_var(true)
	
	for node in nodes:
		if node.has_method("load"):
			node.load(objects[node.save_id])
	
