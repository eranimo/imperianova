class_name Heightmap
extends OpenSimplexNoise

export(Dictionary) var _data
export(int) var _width
export(int) var _height

func _init(width, height):
	_width = width
	_height = height
	_data = {}

func generate(_seed):
	self.seed = _seed
	for x in _width:
		for y in _height:
			var v = self.get_noise_2d(x, y)
			v = clamp(v + 0.5, 0, 1)
			_data[Vector2(x, y)] = v

func get_cell(pos: Vector2):
	return _data[pos]
