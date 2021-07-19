# https://github.com/db0/godot-hexgrid_tileset_demo

"""
	A converter between hex and Godot-space coordinate systems.
	
	The hex grid uses +x => NE and +y => N, whereas
	the projection to Godot-space uses +x => E, +y => S.
	
	We map hex coordinates to Godot-space with +y flipped to be the down vector
	so that it maps neatly to both Godot's 2D coordinate system, and also to
	x,z planes in 3D space.
	
	
	## Usage:
	
	#### var hex_scale = Vector2(...)

		If you want your hexes to display larger than the default 1 x 0.866 units,
		then you can customise the scale of the hexes using this property.
	
	#### func get_hex_center(hex)
	
		Returns the Godot-space Vector2 of the center of the given hex.
		
		The coordinates can be given as either a HexCell instance; a Vector3 cube
		coordinate, or a Vector2 axial coordinate.
	
	#### func get_hex_center3(hex [, y])
	
		Returns the Godot-space Vector3 of the center of the given hex.
		
		The coordinates can be given as either a HexCell instance; a Vector3 cube
		coordinate, or a Vector2 axial coordinate.
		
		If a second parameter is given, it will be used for the y value in the
		returned Vector3. Otherwise, the y value will be 0.
	
	#### func get_hex_at(coords)
	
		Returns HexCell whose grid position contains the given Godot-space coordinates.
		
		The given value can either be a Vector2 on the grid's plane
		or a Vector3, in which case its (x, z) coordinates will be used.
	
"""
extends TileMap
class_name HexMap


var HexCell = preload("./HexCell.gd")
# Duplicate these from HexCell for ease of access
const DIR_N = Vector3(0, 1, -1)
const DIR_NE = Vector3(1, 0, -1)
const DIR_SE = Vector3(1, -1, 0)
const DIR_S = Vector3(0, -1, 1)
const DIR_SW = Vector3(-1, 0, 1)
const DIR_NW = Vector3(-1, 1, 0)
const DIR_ALL = [DIR_N, DIR_NE, DIR_SE, DIR_S, DIR_SW, DIR_NW]
const hex_size = Vector2(64,60) # We define how big the tiles for our hexes are

"""
	Converting between hex-grid and 2D spatial coordinates
"""
func get_hex_center(hex: HexCell) -> Vector2:
	# Returns a hex's centre position on the projection plane
	hex = HexCell.new(hex)
	return map_to_world(hex.get_offset_coords()) + hex_size/2
	
func get_hex_at(coords) -> HexCell:
	# Returns a HexCell at the given Vector2/3 on the projection plane
	# If the given value is a Vector3, its x,z coords will be used
	if typeof(coords) == TYPE_VECTOR3:
		coords = Vector2(coords.x, coords.z)
	return HexCell.new(coords)
	
func get_hex_center3(hex, y=0) -> Vector3:
	# Returns hex's centre position as a Vector3
	var coords = get_hex_center(hex)
	return Vector3(coords.x, y, coords.y)
	