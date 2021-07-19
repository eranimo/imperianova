extends Node
class_name Movement

const component_name = 'Movement'

var target = Vector2(10, 10)
var move_list = []
var target_unreachable = false

func _ready():
	EntitySystem.register_component(self)

func game_process():
	var tilePosition = EntitySystem.get_component(self, "TilePosition")

	if target != null and target_unreachable == false:
		if move_list.size() == 0:
			var path = MapData.find_path(
				tilePosition.tile_pos,
				target
			)
			print(path, tilePosition.tile_pos, target)
			if path.size() == 0:
				# no path to target
				target_unreachable = true
			for cell in path:
				move_list.append(cell)
		if target_unreachable == false:
			var next_tile = move_list.pop_front()
			tilePosition.set_tile_pos(next_tile)
	
