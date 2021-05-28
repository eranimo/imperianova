extends PanelContainer

onready var PlayButton = get_node("Content/Grid/LeftColumn/PlayButton")
onready var DateDisplay = get_node("Content/Grid/LeftColumn/Date")
onready var ChangeSpeed = get_node("Content/Grid/LeftColumn/ChangeSpeed")
onready var Menu = get_node("Content/Grid/RightColumn/Menu")
onready var Game = get_parent().get_parent()

func _ready():
	Game.date_ticks.subscribe(self, "_date_tick")
	Game.is_playing.subscribe(self, "_play_state_changed")
	Game.speed.subscribe(self, "_speed_changed")
	PlayButton.connect("pressed", self, "_play_button_pressed")
	ChangeSpeed.connect("pressed", self, "_speed_button_pressed")

func _date_tick(date: int):
	DateDisplay.text = str(date)

func _play_state_changed(is_playing):
	if is_playing:
		PlayButton.text = "Pause"
	else:
		PlayButton.text = "Play"

func _speed_changed(speed):
	if speed == Game.Speed.SLOW:
		ChangeSpeed.text = "Slow"
	elif speed == Game.Speed.NORMAL:
		ChangeSpeed.text = "Normal"
	elif speed == Game.Speed.FAST:
		ChangeSpeed.text = "Fast"

func _play_button_pressed():
	if Game.is_playing.value:
		Game.is_playing.next(false)
	else:
		Game.is_playing.next(true)

func _speed_button_pressed():
	Game.toggle_speed()
