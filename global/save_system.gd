extends Node

var current_save = null

var dir = Directory.new()

const SAVE_PATH = "user://saves"
const SAVE_FILE = "user://saves/%s"

func _init():
	# create save directory
	if not dir.dir_exists(SAVE_PATH):
		dir.make_dir("user://saves")

func save_game():
	if not current_save:
		return false
	var file = File.new()
	file.open(SAVE_FILE % current_save, File.WRITE)
	var save_nodes = get_tree().get_nodes_in_group("persist")
	for node in save_nodes:
		if node.has_method('save'):
			node.call('save', file)
	file.close()

func get_saves():
	var saves = []
	if dir.open(SAVE_PATH) == OK:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while file_name != "":
			if not dir.current_is_dir():
				saves.append(file_name)
			file_name = dir.get_next()
	else:
		print("An error occurred when trying to access the path.")
	return saves		
