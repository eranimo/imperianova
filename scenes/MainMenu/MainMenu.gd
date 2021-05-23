extends Node2D

func _ready():
	register_buttons()

func register_buttons():
	var buttons = get_tree().get_nodes_in_group("buttons")
	for button in buttons:
		button.connect("pressed", self, "_on_button_pressed", [button.name])

func _on_button_pressed(name):
	match name:
		"Button-NewGame":
			get_tree().change_scene("res://scenes/GameView/GameView.tscn")
