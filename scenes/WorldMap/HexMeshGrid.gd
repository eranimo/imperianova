extends Node2D

var hexes = {}

var is_setup = false

func setup_grid():
	if is_setup:
		return
	for x in range(MapData.CHUNK_SIZE.x):
		for y in range(MapData.CHUNK_SIZE.y):
			var hex = MeshInstance2D.new()
			var hex_center = HexUtils.oddr_offset_to_pixel(x, y)
			hex.position = hex_center
			hex.mesh = HexUtils.hex_mesh
			hex.name = "(%s, %s)" % [x, y]
			hexes[Vector2(x, y)] = hex
			add_child(hex)
	is_setup = true

func set_hex_color(hex: Vector2, color: Color):
	var hex_mesh_inst: MeshInstance2D = hexes[hex]
	hex_mesh_inst.modulate = color
