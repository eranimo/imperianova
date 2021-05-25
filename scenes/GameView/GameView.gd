extends Node2D

var is_menu_open = false

signal game_loaded

const SAVE_PATH = "user://saves"
const SAVE_FILE = "user://saves/%s.res"

func _ready():
	$GameMenu.connect("hide", self, '_on_menu_close')
	$GameMenu.connect("about_to_show", self, '_on_menu_open')
	
	if SaveSystem.current_save:
		load_game(SaveSystem.current_save)
		SaveSystem.current_save = null
	else:
		$Game.generate()
		emit_signal("game_loaded")

# GAME MENU
func _on_menu_close():
	get_tree().paused = false
	is_menu_open = false

func _on_menu_open():
	get_tree().paused = true
	is_menu_open = true

func _exit_tree():
	SaveSystem.current_save = null
	if is_menu_open:
		get_tree().paused = false

func _input(event):
	if event.is_action_pressed("ui_exit"):
		$GameMenu.popup()

# SAVE SYSTEM

func _init():
	# create save directory
	var dir = Directory.new()
	if not dir.dir_exists(SAVE_PATH):
		dir.make_dir("user://saves")

func _set_owner(node, root):
	if node != root:
		node.owner = root
	for child in node.get_children():
		if is_instanced_from_scene(child)==false:
			_set_owner(child, root)
		else:
			child.owner = root

func is_instanced_from_scene(p_node):
	if not p_node.filename.empty():
		return true
	return false

func save_game(save_name):
	var filepath = SAVE_FILE % save_name;
	var root = get_node("Game")
	var packed_scene = PackedScene.new()
	_set_owner(get_node("Game"), get_node("Game"))
	packed_scene.pack(root)
	var exts = ResourceSaver.get_recognized_extensions(packed_scene)
	print(exts)
	var err = ResourceSaver.save(filepath, packed_scene)
	if err != OK:
		print("Failed to save game")

func load_game(save_name):
	remove_child(get_node("Game"))
	yield(get_tree().create_timer(0.01),"timeout")
	var filepath = SAVE_FILE % save_name;
	
	print("Loading save from: ", filepath)
	if not ResourceLoader.exists(filepath):
		print("Failed to find save game")
	
	var packed_scene = ResourceLoader.load(filepath)
	if packed_scene == null:
		print("Failed to load game")
	var new_game = packed_scene.instance()
	add_child(new_game)
	new_game.on_game_loaded()
	emit_signal("game_loaded")
