extends Container

func _ready():
	register_buttons()
	$LoadDialog.connect("load_game", self, "_on_load_game")

func register_buttons():
	var buttons = get_tree().get_nodes_in_group("buttons")
	for button in buttons:
		button.connect("pressed", self, "_on_button_pressed", [button.name])

func _on_button_pressed(name):
	match name:
		"NewGame":
			get_tree().change_scene("res://scenes/GameView/GameView.tscn")
		"LoadGame":
			$LoadDialog.popup_centered()
		"ExitGame":
			get_tree().quit()

func _on_load_game(save_name):
	self.hide()
	SaveSystem.pending_save = save_name
	get_tree().change_scene("res://scenes/GameView/GameView.tscn")
