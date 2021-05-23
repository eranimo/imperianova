extends WindowDialog

onready var ItemList = $ScrollContainer/VBoxContainer/ItemList
onready var NoItemsText = $ScrollContainer/VBoxContainer/NoItemsText
onready var Cancel = $VBoxContainer/HBoxContainer/Cancel
onready var Save = $VBoxContainer/HBoxContainer/Save
onready var SaveNameInput = $VBoxContainer/SaveNameInput


func _ready():
	self.connect("about_to_show", self, '_on_open')
	ItemList.connect("item_selected", self, '_item_selected')
	Cancel.connect("pressed", self, '_on_cancel')
	Save.connect("pressed", self, '_on_save')

func _on_open():
	update_save_list()

func update_save_list():
	# load save games and populate list
	var saves = SaveSystem.get_saves()
	for save in saves:
		print('add item ', save)
		ItemList.add_item(save)
	NoItemsText.visible = bool(len(saves) == 0)

func _item_selected(index):
	SaveNameInput.text = ItemList.get_item_text(index)

func _on_cancel():
	self.hide()

func _on_save():
	SaveSystem.current_save = SaveNameInput.text
	SaveSystem.save_game()
	update_save_list()
	self.hide()
	self.get_parent().hide()
