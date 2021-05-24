extends WindowDialog

onready var ItemList = $Control/ScrollContainer/MarginContainer/VBoxContainer/ItemList
onready var Header = $Control/ScrollContainer/MarginContainer/VBoxContainer/Header
onready var Cancel = $Control/MarginContainer/VBoxContainer/HBoxContainer/Cancel
onready var Save = $Control/MarginContainer/VBoxContainer/HBoxContainer/Save
onready var SaveNameInput = $Control/MarginContainer/VBoxContainer/SaveNameInput

signal save_game(save_name)

func _ready():
	self.connect("about_to_show", self, '_on_open')
	ItemList.connect("item_selected", self, '_item_selected')
	Cancel.connect("pressed", self, 'hide')
	Save.connect("pressed", self, '_on_save')
	
	#self.popup()

func _on_open():
	update_save_list()

func update_save_list():
	# load save games and populate list
	var saves = SaveSystem.get_saves()
	for save in saves:
		ItemList.add_item(save)
	if len(saves) == 0:
		Header.text = 'There are no save games'
		ItemList.hide()
	else:
		Header.text = 'Select a save to overwrite'

func _item_selected(index):
	SaveNameInput.text = ItemList.get_item_text(index)

func _on_save():
	self.hide()
	emit_signal("save_game", SaveNameInput.text)
