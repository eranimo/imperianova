extends Node

"""

Each node that is persisted must be an instanced scene
Each persisted node must have

"""

# The name of the current save game
# if this is null the game has not been saved yet
var current_save = null

var pending_save = null

signal load_complete

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
	print("Save: ", save_name)
	var save_file = File.new()
	save_file.open(_get_save_file(save_name), File.WRITE)
	
	var objects = []
	var nodes = _get_persisted_nodes()
	# TODO: order parent nodes before child nodes
	for node in nodes:
		# Check the node is an instanced scene so it can be instanced again during load.
		if node.filename.empty():
			print("persistent node '%s' is not an instanced scene, skipped" % node.name)
			continue
		
		if !node.has_method("save"):
			print("persistent node '%s' is missing a save() function, skipped" % node.name)
			continue
		objects.append({
			"parent": node.get_parent().get_path(),
			"filename": node.filename,
			"data": node.call('save'),
		})
	
	var save_data = {
		"version": ProjectSettings.get_setting("application/config/version"),
		"objects": objects,
	}
	
	save_file.store_var(save_data, true)
	save_file.close()
	current_save = save_name

func load_game(save_name):
	print("Load: ", save_name)
	var save_file = File.new()
	var file_path = _get_save_file(save_name)

	# TODO: handle save game not existing
	if not save_file.file_exists(file_path):
		return
	
	# free old data
	var nodes = _get_persisted_nodes()
	for node in nodes:
		node.queue_free()

	# open save file
	save_file.open(file_path, File.READ)
	var save_data = save_file.get_var(true)
	save_file.close()
	
	# check version
	# TODO: show confirm when old version is detected
	var current_version = ProjectSettings.get_setting("application/config/version")
	if current_version != save_data["version"]:
		return
	
	# create objects
	for node_data in save_data["objects"]:
		var new_object = load(node_data["filename"]).instance()
		var parent_node = get_node(node_data["parent"])
		if parent_node == null:
			print("Failed to load object (filename: %s   parent: %s)" % [node_data["filename"], node_data["parent"]])
			continue
		parent_node.add_child(new_object)

		for i in node_data["data"]:
			new_object.set(i, node_data["data"][i])

	current_save = save_name
	emit_signal("load_complete")
