extends Node

const SIZE = 32.0 # distance from hex center to corner
const hex_width = 2 * SIZE
const hex_height = sqrt(3) * SIZE

var hex_mesh

func flat_hex_corner(center, i):
	var angle_deg = 60 * i
	var angle_rad = PI / 180 * angle_deg
	return Vector2(
		center.x + SIZE * cos(angle_rad),
		center.y + SIZE * sin(angle_rad)
	)

func oddr_offset_to_pixel(row, col):
	row = int(row)
	col = int(col)
	var x = SIZE * 3/2 * col
	var y = SIZE * sqrt(3) * (row + 0.5 * (col & 1))
	return Vector2(x, y)

func create_hex_mesh():
	var vertices = PoolVector2Array()
	var center = Vector2(HexUtils.hex_width / 2, HexUtils.hex_height / 2)
	
	for i in range(6):
		var j = (i + 1) % 6
		var c1 = HexUtils.flat_hex_corner(center, i)
		var c2 = HexUtils.flat_hex_corner(center, j)
		vertices.push_back(center)
		vertices.push_back(Vector2(c1.x, c1.y))
		vertices.push_back(Vector2(c2.x, c2.y))
	
	var arr_mesh = ArrayMesh.new()
	var arrays = []
	arrays.resize(ArrayMesh.ARRAY_MAX)
	arrays[ArrayMesh.ARRAY_VERTEX] = vertices
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return arr_mesh

func _ready():
	hex_mesh = create_hex_mesh()
