extends HexMap

signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)

var _last_hovered_tile_pos = null

onready var OverlayTexture = preload("res://assets/textures/overlay.tres")
onready var MapChunk = preload("res://scenes/WorldMap/MapChunk.tscn")

func _ready():
	MapManager.connect_map(self)
	MapManager.connect("tile_hovered", self, "_on_tile_hover")
	MapManager.selected_tile.subscribe(self, "_update_selected_tile")

func _exit_tree():
	MapManager.selected_tile.unsubscribe(self)

func render():
	var chunk_width = MapData.world.map_width / MapData.CHUNK_SIZE.x
	var chunk_height = MapData.world.map_height / MapData.CHUNK_SIZE.y
	for cx in chunk_width:
		for cy in chunk_height:
			var map_chunk = MapChunk.instance()
			map_chunk.chunk_position = Vector2(cx, cy)
			map_chunk.name = "MapChunk (%d, %d)" % [cx, cy]
			$MapChunks.add_child(map_chunk)
			var first_hex = Vector2(cx * MapData.CHUNK_SIZE.x, cy * MapData.CHUNK_SIZE.y)
			map_chunk.position = map_to_world(to_global(first_hex))

	# DEBUG: render bitmask IDs on tiles
	for pos in MapData.tiles():
		$GridLines.set_cellv(pos, 0)

func _unhandled_input(event) -> void:
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
		$TileUI/HoverTile.hide()
	else:
		$TileUI/HoverTile.show()
		$TileUI/HoverTile.position = world_pos

func _update_selected_tile(selected_tile):
	if selected_tile == null:
		$TileUI/SelectedTile.hide()
	else:
		$TileUI/SelectedTile.show()
		$TileUI/SelectedTile.position = map_to_world(selected_tile)
