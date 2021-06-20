extends HexMap

signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)

var _last_hovered_tile_pos = null

onready var OverlayTexture = preload("res://assets/textures/overlay.tres")

func _ready():
	MapManager.connect_map(self)
	MapManager.connect("tile_hovered", self, "_on_tile_hover")

func render():
	for x in range(MapData.map_width):
		for y in range(MapData.map_height):
			$Terrain.set_tile(Vector2(x, y))
	print("Tileset tile count: ", $Terrain.tile_cache.size())
	# DEBUG: render bitmask IDs on tiles
	for pos in MapData.tiles:
		$GridLines.set_cellv(pos, 0)
		
		if false:
			var label_container = Node2D.new()
			label_container.position = map_to_world(pos) + Vector2(32, 32)
			var label = Label.new()
			label.text = str(MapData.get_tile_bitmask(pos))
			label_container.add_child(label)
			add_child(label_container)

	$MapOverlay.render()
	$Terrain.render()

func _input(event) -> void:
	var grid_pos: Vector2 = world_to_map(get_global_mouse_position()) 
	var hexCell: HexCell = get_hex_at(grid_pos)
	var hexWorldPos: Vector2 = map_to_world(hexCell.get_offset_coords())
	if event.is_action_pressed("ui_select"): 
		emit_signal("tile_pressed", hexCell.offset_coords)
	
	if event.is_action_pressed("map_toggle_grid"):
		if $GridLines.visible:
			$GridLines.hide()
		else:
			$GridLines.show()

	if _last_hovered_tile_pos == null or \
		not hexCell.offset_coords.is_equal_approx(_last_hovered_tile_pos):
		emit_signal("tile_hovered", hexCell.offset_coords, hexWorldPos)
		_last_hovered_tile_pos = hexCell.offset_coords

func _on_tile_hover(tile_pos, world_pos):
	if tile_pos == null:
		$TileUI.hide()
	else:
		$TileUI.show()
		$TileUI.update_highlight_pos(world_pos)
