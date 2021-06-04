class_name WorldNoise
extends OpenSimplexNoise

export(Dictionary) var _data
export(int) var _width
export(int) var _height

func _init(width, height):
	_width = width
	_height = height
	_data = {}
	self.octaves = 7
	self.period = 1.0
	self.persistence = 0.5

func generate(_seed):
	self.seed = _seed
	for x in _width:
		for y in _height:
			var long = ((float(x) / _width) * 360) - 180
			var lat = ((-float(y) / _height) * 180) + 90
			var inc = ((lat + 90.0) / 180.0) * PI
			var azi = ((long + 180.0) / 360.0) * (2 * PI)
			var nx = sin(inc) * cos(azi)
			var ny = sin(inc) * sin(azi)
			var nz = cos(inc)
			var v = self.get_noise_3d(nx, ny, nz)
			v = clamp((v + 1) / 2, 0, 1)
			_data[Vector2(x, y)] = v

func get_cell(pos: Vector2):
	return _data[pos]
