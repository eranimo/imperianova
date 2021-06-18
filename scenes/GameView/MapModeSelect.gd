extends OptionButton


func _ready():
	for item in MapManager.map_mode_titles:
		add_item(MapManager.map_mode_titles[item], item)


func _on_MapModeSelect_item_selected(index):
	MapManager.set_map_mode(index)
