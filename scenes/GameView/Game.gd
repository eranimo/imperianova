extends Node

var date_ticks setget _set_date_ticks

signal date_tick(date_ticks)

onready var DateText = $CanvasLayer/Control/MarginContainer/HBoxContainer/Date

func _ready():
	SaveSystem.connect("load_complete", self, "setup_game")

func setup_game():
	print("Game loaded from file")

func generate():
	var size = 100
	print("Generating world ", (size * (size * 2)))
	date_ticks = 0
	# DateText.text = str(0)
	$World.generate({
		"map_seed": rand_range(0, 100),
		"size": size,
	})

func _set_date_ticks(ticks):
	print("Set ticks ", ticks)
	DateText.text = str(ticks)
	emit_signal("date_tick", ticks)

func to_dict():
	return {
		"date_ticks": date_ticks
	}

func from_dict(dict):
	date_ticks = dict["date_ticks"]
