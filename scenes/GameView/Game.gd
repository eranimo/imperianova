extends Node
class_name Game

var ReactiveState = preload("res://scripts/ReactiveState.gd")
var NameGen = preload("res://scripts/NameGen.gd")

enum Speed {
	SLOW,
	NORMAL,
	FAST
}

const TICKS_PER_DAY = 4
const speed_ticks = {
	Speed.SLOW: 4 * TICKS_PER_DAY,
	Speed.NORMAL: 2 * TICKS_PER_DAY,
	Speed.FAST: 1 * TICKS_PER_DAY,
}

# State
var date_ticks = ReactiveState.new(0)
var speed = ReactiveState.new(Speed.NORMAL)
var is_playing = ReactiveState.new(false)
var entities = []

onready var GameState = get_node("GameState")

var _ticks_in_day = 0

func _ready():
	SaveSystem.connect("load_complete", self, "setup_game")
	EntitySystem.CURRENT_TICKS_PER_DAY = speed_ticks[speed.value]

func _exit_tree():
	SaveSystem.disconnect("load_complete", self, "setup_game")

func _process(_delta):
	GameState.Process()
	if not is_playing.value:
		return
	if _ticks_in_day == 0:
		var ticks_left = speed_ticks[speed.value]
		date_ticks.next(date_ticks.value + 1)
		EntitySystem.update(date_ticks.value)
		GameState.Update(date_ticks.value)
		_ticks_in_day = ticks_left
	else:
		_ticks_in_day -= 1

func _input(event):
	if event.is_action_pressed("ui_playstate"):
		is_playing.next(!is_playing.value)
		get_tree().set_input_as_handled()

func toggle_speed():
	if speed.value == Speed.SLOW:
		speed.next(Speed.NORMAL)
	elif speed.value == Speed.NORMAL:
		speed.next(Speed.FAST)
	else:
		speed.next(Speed.SLOW)
	EntitySystem.CURRENT_TICKS_PER_DAY = speed_ticks[speed.value]

func setup_game():
	print("Game loaded from file")
	EntitySystem.setup()

func generate():
	var size = 150
	# var gen = NameGen.new().add_from_file('greek')
	# print(gen.generate_names(10))
	print("Generating world ", (size * (size * 2)))
	$GameWorld.generate({
		"map_seed": rand_range(0, 100),
		"size": size,
		"sealevel": 140,
	})
	
func _on_menu_pressed():
	get_parent().open_menu()

func to_dict():
	return {
		"date_ticks": date_ticks.value,
		"speed": speed.value,
	}

func from_dict(dict):
	date_ticks.next(dict["date_ticks"])
	speed.next(dict["speed"])
