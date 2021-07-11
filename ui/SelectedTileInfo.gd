extends WindowDialog

onready var Title = $ScrollContainer/VBoxContainer/MarginContainer/Title

func _ready():
	MapManager.selected_tile.subscribe(self, "_update_ui")

func _exit_tree():
	MapManager.selected_tile.unsubscribe(self)

func _update_ui(selected_tile):
	if selected_tile == null:
		self.visible = false
		return
	self.visible = true
	Title.text = "Tile %s" % str(selected_tile)

func _on_popup_hide():
	MapManager.selected_tile.next(null)
