extends Node

const SIZE = 32.0 # distance from hex center to corner
const hex_width = 2 * SIZE
const hex_height = sqrt(3) * SIZE
const _origin = Vector2(0, 0)
var hex_mesh: Mesh


const _orientation = [
	3.0 / 2.0, 0.0, sqrt(3.0) / 2.0, sqrt(3.0),
	2.0 / 3.0, 0.0, -1.0 / 3.0, sqrt(3.0) / 3.0,
	0.0
]

func flat_hex_corner(center, i) -> Vector2:
	var angle_deg = 60 * i
	var angle_rad = PI / 180 * angle_deg
	return Vector2(
		center.x + SIZE * cos(angle_rad),
		center.y + SIZE * sin(angle_rad)
	)

func round_hex(cube: Vector3):
	var rx = round(cube.x)
	var ry = round(cube.y)
	var rz = round(cube.z)

	var x_diff = abs(rx - cube.x)
	var y_diff = abs(ry - cube.y)
	var z_diff = abs(rz - cube.z)

	if x_diff > y_diff and x_diff > z_diff:
		rx = -ry-rz
	elif y_diff > z_diff:
		ry = -rx-rz
	else:
		rz = -rx-ry

	return Vector3(rx, ry, rz) # cube

func offset_to_axial(hex: Vector2):
	return Vector2(
		hex.x,
		hex.y - floor(hex.x / 2)
	)

func axial_to_offset(hex: Vector2):
	return Vector2(
		hex.x,
		hex.y + floor(hex.x / 2)
	)

func cube_to_offset(cube):
	var col = cube.x
	var row = cube.z + (cube.x - (int(cube.x) & 1)) / 2
	return Vector2(col, row)

func offset_to_cube(hex):
	var x = hex.x
	var z = hex.y - (hex.x - (int(hex.x) & 1)) / 2
	var y = -x-z
	return Vector3(x, y, z)

func hex_to_pixel(hex_offset):
	var hex = offset_to_axial(hex_offset)
	var x = (_orientation[0] * hex.x + _orientation[1] * hex.y) * SIZE
	var y = (_orientation[2] * hex.x + _orientation[3] * hex.y) * SIZE
	return Vector2(x + _origin.x, y + _origin.y);

func pixel_to_hex(hex_offset):
	var pt = Vector2(
		(hex_offset.x - _origin.x) / SIZE,
		(hex_offset.y - _origin.y) / SIZE
	);
	var q = _orientation[4] * pt.x + _orientation[5] * pt.y;
	var r = _orientation[6] * pt.x + _orientation[7] * pt.y;
	return cube_to_offset(round_hex(Vector3(q, r, -q - r)))

func create_hex_mesh() -> ArrayMesh:
	var vertices = PoolVector2Array()
	var center = Vector2(hex_width / 2, hex_height / 2)
	
	for i in range(6):
		var j = (i + 1) % 6
		var c1 = flat_hex_corner(center, i)
		var c2 = flat_hex_corner(center, j)
		vertices.push_back(center)
		vertices.push_back(c1)
		vertices.push_back(c2)
	
	var arr_mesh = ArrayMesh.new()
	var arrays = []
	arrays.resize(ArrayMesh.ARRAY_MAX)
	arrays[ArrayMesh.ARRAY_VERTEX] = vertices
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return arr_mesh

func _ready():
	hex_mesh = create_hex_mesh()
