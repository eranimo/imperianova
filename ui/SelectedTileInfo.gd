extends WindowDialog

onready var Title = $Control/ScrollContainer/MarginContainer/VBoxContainer/Title
onready var Details = $Control/ScrollContainer/MarginContainer/VBoxContainer/Details

func _ready():
	MapManager.selected_tile.subscribe(self, "_update_ui")
	Details.set_columns([
		{
			"label": "ID",
			"key": "id"
		},
		{
			"label": "Size",
			"key": "size"
		}
	])
	Details.add_item({
		"id": 0,
		"size": 100 
	})
	Details.add_item({
		"id": 1,
		"size": 1000
	})

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


func _notification(what):
	if what == NOTIFICATION_RESIZED:
		get_tree().root.update_worlds();
		force_update_transform();
		update();
