extends KinematicBody2D

var target = Vector2(10, 10)
var move_list = []
var tile_pos setget set_tile_pos
var target_unreachable = false

func set_tile_pos(value: Vector2):
	print("Set tile pos ", value)
	tile_pos = value
	position = MapManager.map.map_to_world(tile_pos)

func _process(_delta):
	if tile_pos != null and target != null:
		if target_unreachable:
			return
		if move_list.size() == 0:
			print("Cost: ", MapManager.map.get_hex_cost(target))
			var path = MapManager.map.find_path(
				MapManager.map.get_hex_at(tile_pos),
				MapManager.map.get_hex_at(target)
			)
			print(path, tile_pos, target)
			if path.size() == 0:
				# no path to target
				target_unreachable = true
			for cell in path:
				move_list.append(cell.offset_coords)
		if target_unreachable == false:
			var next_tile = move_list.pop_front()
			print("Move to %s" % str(next_tile))
			tile_pos = next_tile
