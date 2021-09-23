extends Node2D


const SIZE = 5.0 # distance from hex center to corner
const hex_width = 2 * SIZE
const hex_height = sqrt(3) * SIZE

func flat_hex_corner(center, i):
	var angle_deg = 60 * i
	var angle_rad = PI / 180 * angle_deg
	return Vector2(
		center.x + SIZE * cos(angle_rad),
		center.y + SIZE * sin(angle_rad)
	)

func create_hex_mesh():
	var vertices = PoolVector2Array()
	var center = Vector2(hex_width / 2, hex_height / 2)
	
	for i in range(6):
		var j = (i + 1) % 6
		var c1 = flat_hex_corner(center, i)
		var c2 = flat_hex_corner(center, j)
		vertices.push_back(center)
		vertices.push_back(Vector2(c1.y, c1.x))
		vertices.push_back(Vector2(c2.y, c2.x))
	
	var arr_mesh = ArrayMesh.new()
	var arrays = []
	arrays.resize(ArrayMesh.ARRAY_MAX)
	arrays[ArrayMesh.ARRAY_VERTEX] = vertices
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return arr_mesh

func oddr_offset_to_pixel(row, col):
	var x = SIZE * sqrt(3) * (col + 0.5 * (row & 1))
	var y = SIZE * 3/2 * row
	return Vector2(x, y)

func _ready():
	var hex_mesh = create_hex_mesh()
	
	var rand_generate = RandomNumberGenerator.new()
	rand_generate.randomize()
	var start = OS.get_ticks_msec()
	
	for x in range(300):
		for y in range(150):
			var hex = MeshInstance2D.new()
			var hex_center = oddr_offset_to_pixel(x, y)
			hex.position = hex_center

			# random color
			var c = rand_generate.randf_range(0, 1)
			hex.modulate = Color(c, c, c)
			hex.mesh = hex_mesh
			hex.name = "(%s, %s)" % [x, y]
			add_child(hex)
	
	print("Generated in %s ms" % str(OS.get_ticks_msec() - start))
