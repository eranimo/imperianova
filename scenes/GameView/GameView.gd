extends Camera2D

var fixed_toggle_point = Vector2(0,0)


func _ready():
	var map = get_node("Map")
	map.call("setup_map", 100, 50)

func _process(delta):
	# This happens once 'move_map' is pressed
	if Input.is_action_just_pressed('move_map'):
		var ref = get_viewport().get_mouse_position()
		fixed_toggle_point = ref
	# This happens while 'move_map' is pressed
	if Input.is_action_pressed('move_map'):
		slide_map_around()

# slides the map around
func slide_map_around():
	var ref = get_viewport().get_mouse_position()
	self.global_position.x += (ref.x - fixed_toggle_point.x) / 20
	self.global_position.y += (ref.y - fixed_toggle_point.y) / 20
