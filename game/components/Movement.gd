extends Node
class_name Movement

const component_name = 'Movement'

var target = Vector2(50, 50)
var move_list = []
var target_unreachable = false
var path_visible = false setget set_path_visible

onready var path2D: SmoothPath = $Path2D

func _ready():
	EntitySystem.register_component(self)

func update_move_path():
	var tilePosition = EntitySystem.get_component(self, "TilePosition")
	path2D.curve.clear_points()
	path2D.curve.add_point(MapManager.world_map.get_hex_center(MapManager.world_map.get_hex_at(tilePosition.tile_pos)))
	for pos in move_list:
		var cell = MapManager.world_map.get_hex_at(pos)
		var center_pos = MapManager.world_map.get_hex_center(cell)
		path2D.curve.add_point(center_pos)
	path2D.smooth(5)
	path2D.update()

func set_path_visible(value):
	path_visible = value
	path2D.visible = path_visible

func game_process():
	var tilePosition = EntitySystem.get_component(self, "TilePosition")

	if target != null and target_unreachable == false:
		if move_list.size() == 0:
			var path = MapData.find_path(
				tilePosition.tile_pos,
				target
			)
			if path.size() == 0:
				# no path to target
				target_unreachable = true
			move_list = Array(path)
		if target_unreachable == false:
			var next_tile = move_list.pop_front()
			tilePosition.set_tile_pos(next_tile)
			if tilePosition.tile_pos.is_equal_approx(target):
				target = null
		update_move_path()
	
