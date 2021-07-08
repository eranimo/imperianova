class_name WorldNoise
extends OpenSimplexNoise

export(int) var _width
export(int) var _height

func _init(width, height, seed_):
	_width = width
	_height = height
	self.octaves = 7
	self.period = 1.0
	self.persistence = 0.5
	self.seed = seed_

func get_cell(pos: Vector2):
	var long = ((float(pos.x) / _width) * 360) - 180
	var lat = ((-float(pos.y) / _height) * 180) + 90
	var inc = ((lat + 90.0) / 180.0) * PI
	var azi = ((long + 180.0) / 360.0) * (2 * PI)
	var nx = sin(inc) * cos(azi)
	var ny = sin(inc) * sin(azi)
	var nz = cos(inc)
	var v = self.get_noise_3d(nx, ny, nz)
	v = clamp((v + 1) / 2, 0, 1)
	return v
