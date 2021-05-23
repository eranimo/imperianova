extends Camera2D

var zoom_step = 0.1
var min_zoom = 0.5
var max_zoom = 2.0

var pan_speed = 600

var _mouse_captured = false
func _input(event):
	# mousewheel zoom
	if event is InputEventMouseButton:
		if event.is_action_pressed("view_zoom_in"):
			zoom /= 1 + zoom_step
		if event.is_action_pressed("view_zoom_out"):
			zoom *= 1 + zoom_step
	
	if event.is_action_pressed("view_pan_mouse"):
		_mouse_captured = true
	elif event.is_action_released("view_pan_mouse"):
		_mouse_captured = false
	
	if _mouse_captured && event is InputEventMouseMotion:
		position -= event.relative * zoom #opposite to relative motion, like we're grabbing the map

func _process(delta):
	var panning = Vector2()
	if Input.is_action_pressed("view_pan_up"):
		panning.y -= 1
	if Input.is_action_pressed("view_pan_down"):
		panning.y += 1
	if Input.is_action_pressed("view_pan_left"):
		panning.x -= 1
	if Input.is_action_pressed("view_pan_right"):
		panning.x += 1
	
	if panning.length_squared() > 0:
		position += panning.normalized() * pan_speed * delta * zoom
