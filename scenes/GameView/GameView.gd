extends Node2D

var is_menu_open = false

signal game_loaded

const SAVE_PATH = "user://saves"
const SAVE_FILE = "user://saves/%s.res"

func _ready():
	print("GameView ready")
	$GameMenu.connect("hide", self, '_on_menu_close')
	$GameMenu.connect("about_to_show", self, '_on_menu_open')
	
	if SaveSystem.pending_save:
		SaveSystem.load_game(SaveSystem.pending_save)
	
	if not SaveSystem.current_save:
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

func open_menu():
	$GameMenu.popup()

func _input(event):
	if event.is_action_pressed("ui_exit"):
		open_menu()
	get_tree().is_input_handled()
