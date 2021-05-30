extends HexMap

signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)

var _last_hovered_tile_pos = null

func _ready():
	MapManager.connect_map(self)
	MapManager.connect("tile_hovered", self, "_on_tile_hover")

func render():
	# DEBUG: render bitmask IDs on tiles
	if true:
		for x in range(MapData.map_width):
			for y in range(MapData.map_height):
				var pos = Vector2(x, y)
				$GridLines.set_cellv(pos, 0)
				var label_container = Node2D.new()
				label_container.position = map_to_world(pos) + Vector2(32, 32)
				var label = Label.new()
				label.text = str(MapData.get_terrain_bitmask(pos))
				label_container.add_child(label)
				add_child(label_container)

func _input(event) -> void:
	var grid_pos: Vector2 = world_to_map(get_global_mouse_position()) 
	var hexCell: HexCell = get_hex_at(grid_pos)
	var hexWorldPos: Vector2 = map_to_world(hexCell.get_offset_coords())
	if event.is_action_pressed("ui_select"): 
		emit_signal("tile_pressed", hexCell.offset_coords)

	if _last_hovered_tile_pos == null or \
		not hexCell.offset_coords.is_equal_approx(_last_hovered_tile_pos):
		emit_signal("tile_hovered", hexCell.offset_coords, hexWorldPos)
		_last_hovered_tile_pos = hexCell.offset_coords

func _on_tile_hover(tile_pos, world_pos):
	if tile_pos == null:
		$HexGrid/Highlight.hide()
	else:
		$HexGrid/Highlight.show()
		$HexGrid.update_highlight_pos(world_pos)

func set_tile(tile_pos, tile_id):
	$Terrain.set_tile(tile_pos, tile_id)
