extends Spatial

onready var HTerrainData = preload("res://addons/zylann.hterrain/hterrain_data.gd")


onready var Terrain = get_node("Terrain")
export(int) var noise_multiplier = 50.0
export(int) var noise_octaves = 7
export(int) var noise_period = 1.0
export(int) var noise_persistence = 0.5
export(int) var noise_freq = 5
export(int) var slope_amount = 4.0

var is_ready = false

# Called when the node enters the scene tree for the first time.
func _ready():
	is_ready = true
	generate()

func generate():
	print("[MapChunk] generate")
	var terrain_data = HTerrainData.new()
	terrain_data.resize(513)
	var noise = OpenSimplexNoise.new()
	var heightmap: Image = terrain_data.get_image(HTerrainData.CHANNEL_HEIGHT)
	var normalmap: Image = terrain_data.get_image(HTerrainData.CHANNEL_NORMAL)
	var splatmap: Image = terrain_data.get_image(HTerrainData.CHANNEL_SPLAT)

	heightmap.lock()
	normalmap.lock()
	splatmap.lock()

	# Generate terrain maps
	# Note: this is an example with some arbitrary formulas,
	# you may want to come up with your owns
	for z in heightmap.get_height():
		for x in heightmap.get_width():
			# Generate height
			var new_x = x / noise_freq
			var new_z = z / noise_freq
			var h = noise_multiplier * noise.get_noise_2d(new_x, new_z)

			# Getting normal by generating extra heights directly from noise,
			# so map borders won't have seams in case you stitch them
			var h_right = noise_multiplier * noise.get_noise_2d(new_x + 0.1, new_z)
			var h_forward = noise_multiplier * noise.get_noise_2d(new_x, new_z + 0.1)
			var normal = Vector3(h - h_right, 0.1, h_forward - h).normalized()

			# Generate texture amounts
			var splat = splatmap.get_pixel(x, z)
			var slope = 4.0 * normal.dot(Vector3.UP) - 2.0
			var sand_amount = clamp(1.0 - slope, 0.0, 1.0)
			var ocean_amount = clamp(0.0 - h, 0.0, 1.0)
			splat = splat.linear_interpolate(Color(0,1,0,0), sand_amount)
			splat = splat.linear_interpolate(Color(0,0,1,0), ocean_amount)

			heightmap.set_pixel(x, z, Color(h, 0, 0))
			normalmap.set_pixel(x, z, HTerrainData.encode_normal(normal))
			splatmap.set_pixel(x, z, splat)

	# Commit modifications so they get uploaded to the graphics card
	var modified_region = Rect2(Vector2(), heightmap.get_size())
	terrain_data.notify_region_change(modified_region, HTerrainData.CHANNEL_HEIGHT)
	terrain_data.notify_region_change(modified_region, HTerrainData.CHANNEL_NORMAL)
	terrain_data.notify_region_change(modified_region, HTerrainData.CHANNEL_SPLAT)

	Terrain.set_data(terrain_data)
