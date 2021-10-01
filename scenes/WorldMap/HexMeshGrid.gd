extends Node2D

var hexes = {}
var multi_mesh: MultiMesh
var is_setup = false

func setup_grid():
	if is_setup:
		return

	var mesh_instance = MultiMeshInstance2D.new()
	
	multi_mesh = MultiMesh.new()
	multi_mesh.transform_format = MultiMesh.TRANSFORM_2D
	multi_mesh.color_format = MultiMesh.COLOR_FLOAT
	multi_mesh.custom_data_format = MultiMesh.CUSTOM_DATA_NONE
	multi_mesh.instance_count = int(MapData.CHUNK_SIZE.x) * int(MapData.CHUNK_SIZE.y)
	multi_mesh.visible_instance_count = multi_mesh.instance_count
	multi_mesh.mesh = HexUtils.hex_mesh

	mesh_instance.multimesh = multi_mesh
	add_child(mesh_instance)

	var i = 0
	for x in range(MapData.CHUNK_SIZE.x):
		for y in range(MapData.CHUNK_SIZE.y):
			var hex_center = HexUtils.oddq_offset_to_pixel(y, x)
			var transform = Transform2D(0, hex_center)
			multi_mesh.set_instance_transform_2d(i, transform)
			hexes[Vector2(x, y)] = i
			i += 1
	is_setup = true

func set_hex_color(hex: Vector2, color: Color):
	var instance = hexes[hex]
	multi_mesh.set_instance_color(instance, color)
