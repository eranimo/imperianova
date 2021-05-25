extends WindowDialog

onready var ItemList = $Container/ScrollContainer/MarginContainer/VBoxContainer/ItemList
onready var ButtonCancel = $Container/MarginContainer/HBoxContainer/Cancel
onready var ButtonLoad = $Container/MarginContainer/HBoxContainer/LoadGame

signal load_game(save_name)

var selected_save

func _ready():
	#self.popup()
	self.connect("about_to_show", self, '_on_open')
	ItemList.connect("item_selected", self, "_on_select_item")
	ButtonCancel.connect("pressed", self, 'hide')
	ButtonLoad.connect("pressed", self, "_on_press_load")

func _on_open():
	ItemList.clear()
	var saves = SaveSystem.get_saves()
	for save in saves:
		ItemList.add_item(save)
		
	if len(saves) == 0:
		ItemList.hide()

func _on_select_item(index):
	selected_save = ItemList.get_item_text(index)
	
func _on_press_load():
	self.hide()
	emit_signal("load_game", selected_save)
