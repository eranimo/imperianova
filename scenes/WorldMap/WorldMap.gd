extends HexMap

signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)

var _last_hovered_tile_pos = null

func _ready():
	MapManager.connect_map(self)
	MapManager.connect("tile_hovered", self, "_on_tile_hover")

func _input(event) -> void:
	var grid_pos: Vector2 = world_to_map(get_global_mouse_position()) 
	var hexCell: HexCell = get_hex_at(grid_pos)
	var hexWorldPos: Vector2 = map_to_world(hexCell.get_offset_coords())
	if event.is_action_pressed("ui_select"): 
		emit_signal("tile_pressed", hexCell.offset_coords)

	if _last_hovered_tile_pos == null or not hexCell.offset_coords.is_equal_approx(_last_hovered_tile_pos):
		emit_signal("tile_hovered", hexCell.offset_coords, hexWorldPos)
		_last_hovered_tile_pos = hexCell.offset_coords

func _on_tile_hover(tile_pos, world_pos):
	if tile_pos == null:
		$HexGrid/Highlight.hide()
	else:
		$HexGrid/Highlight.show()
		$HexGrid.update_highlight_pos(world_pos)
