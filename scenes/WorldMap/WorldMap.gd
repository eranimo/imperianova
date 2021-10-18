extends Node2D

signal tile_pressed(tile_pos)
signal tile_hovered(tile_pos, world_pos)

var _last_hovered_tile_pos = null

var is_dragging = false
var selected = []
var drag_start = Vector2.ZERO  
var select_rect = RectangleShape2D.new()

onready var OverlayTexture = preload("res://assets/textures/overlay.tres")
onready var MapChunk = preload("res://scenes/WorldMap/MapChunk.tscn")
onready var Unit = preload("res://scenes/WorldMap/Unit.tscn")

func _init():
	MapManager.connect_map(self)

func _ready():
	MapManager.connect("camera_moved", self, "_update_grid_visibility")
	MapManager.connect("tile_hovered", self, "_on_tile_hover")
	MapManager.selected_tile.subscribe(self, "_update_selected_tile")

	# for _i in range(5):
	# 	var unit = Unit.instance()
	# 	unit.setup(Vector2(randi()%50+1, randi()%50+1))
	# 	$Units.add_child(unit)

func _exit_tree():
	MapManager.selected_tile.unsubscribe(self)

func render():
	var chunk_width = floor(MapData.game_world.map_width / MapData.CHUNK_SIZE.y)
	var chunk_height = floor(MapData.game_world.map_height / MapData.CHUNK_SIZE.x)
	for cx in chunk_width:
		for cy in chunk_height:
			var map_chunk = MapChunk.instance()
			map_chunk.chunk_position = Vector2(cx, cy)
			map_chunk.name = "MapChunk (%d, %d)" % [cx, cy]
			$MapChunks.add_child(map_chunk)
			var first_hex = Vector2(cx * MapData.CHUNK_SIZE.x, cy * MapData.CHUNK_SIZE.y)
			map_chunk.position = HexUtils.hex_to_pixel(first_hex)

func _unhandled_input(event) -> void:
	var pos = get_global_mouse_position() - Vector2(HexUtils.SIZE, HexUtils.SIZE)
	prints('Pos:', pos)
	var grid_pos = HexUtils.pixel_to_hex(pos)
	prints('Hex', grid_pos)
	var hexWorldPos = HexUtils.hex_to_pixel(grid_pos)

	if event.is_action_pressed("ui_select"):
		is_dragging = true
		drag_start = get_global_mouse_position()

	if event.is_action_released("ui_select"):

		if get_global_mouse_position().is_equal_approx(drag_start):
			emit_signal("tile_pressed", grid_pos)

		if is_dragging:
			is_dragging = false
			$TileUI.update_selection_rect(null)
			var drag_end = get_global_mouse_position()
			select_rect.extents = (drag_end - drag_start) / 2
			var space = get_world_2d().direct_space_state
			var query = Physics2DShapeQueryParameters.new()
			query.set_shape(select_rect)
			query.transform = Transform2D(0, (drag_end + drag_start) / 2)
			selected = space.intersect_shape(query)
			print(selected)

			if not Input.is_key_pressed(KEY_SHIFT):
				MapManager.clear_selected_units()
			for item in selected:
				MapManager.select_unit(item.collider)
		
	if event is InputEventMouseMotion:
		if is_dragging:
			$TileUI.update_selection_rect(Rect2(drag_start, get_global_mouse_position() - drag_start))
		elif _last_hovered_tile_pos == null or \
			not grid_pos.is_equal_approx(_last_hovered_tile_pos):
			emit_signal("tile_hovered", grid_pos, hexWorldPos)
			_last_hovered_tile_pos = grid_pos
	
	if event.is_action_pressed("map_toggle_grid"):
		# TODO: implement toggling grid
		pass

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
		$TileUI/SelectedTile.position = HexUtils.hex_to_pixel(selected_tile)
