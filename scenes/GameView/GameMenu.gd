extends Popup

func _ready():
	register_buttons()
	
	$SaveDialog.connect("save_game", self, "_on_save_game")
	$LoadDialog.connect("load_game", self, "_on_load_game")

func register_buttons():
	var buttons = get_tree().get_nodes_in_group("buttons")
	for button in buttons:
		button.connect("pressed", self, "_on_button_pressed", [button.name])

func _on_button_pressed(name):
	match name:
		"Continue":
			self.hide()
		"SaveGame":
			$SaveDialog.popup_centered()
		"LoadGame":
			$LoadDialog.popup_centered()
		"MainMenu":
			get_tree().change_scene("res://scenes/MainMenu/MainMenu.tscn")
		"ExitGame":
			get_tree().quit()

func _on_save_game(save_name):
	self.hide()
	SaveSystem.save_game(save_name)

func _on_load_game(save_name):
	self.hide()
	SaveSystem.load_game(save_name)
