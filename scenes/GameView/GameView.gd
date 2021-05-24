extends Node2D

var map_size = 75

const WorldMap = preload("res://resources/WorldMap/WorldMap.gd")

var world_map
var is_menu_open = false

func _ready():
	print("GameView")
	$GameMenu.connect("hide", self, '_on_menu_close')
	$GameMenu.connect("about_to_show", self, '_on_menu_open')
	
	SaveSystem.connect("game_load", self, "_on_game_load")

	if SaveSystem.current_save == null:
		$LoadingContainer.loading = true
		$LoadingContainer.loading_step = 'First'
		world_map = WorldMap.new(map_size, rand_range(0, 100))
		$MapViewport.setup_map(world_map)
		
		yield(get_tree().create_timer(1.0), "timeout")
		
		$LoadingContainer.loading = false

func _on_game_load():
	print("Loaded", world_map.name)
	$MapViewport.setup_map(world_map)

func save(file: File):
	file.store_var(world_map, true)
	
func load(file: File):
	world_map = file.get_var(true)
	print("load world", world_map)

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
