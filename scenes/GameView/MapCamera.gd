extends Camera2D

signal camera_moved

var min_zoom = 0.5
var pan_speed = 600
var last_position = Vector2()
var _mouse_captured = false
var zoom_speed = 0.25
var panning = false

func _unhandled_input(event):
	# Mouse zooming
	if event.is_action_released('view_zoom_in'):
		zoom_camera(-zoom_speed, event.position)
	if event.is_action_released('view_zoom_out'):
		zoom_camera(zoom_speed, event.position)
	
	# trackpad zooming
	if event is InputEventMagnifyGesture:
		if event.factor < 1:
			zoom_camera(-zoom_speed, event.position)
		else:
			zoom_camera(zoom_speed, event.position)
	
	# trackpad panning
	if event is InputEventPanGesture:
		position += event.delta * 25

	# Panning
	if event.is_action_pressed("view_pan_mouse"):
		panning = true
	elif event.is_action_released("view_pan_mouse"):
		panning = false
	
	# Update
	if event is InputEventMouseMotion and panning == true:
		offset -= event.relative * zoom

func _process(delta):
	var speed = 1
	if Input.is_action_pressed("view_pan_fast"):
		speed = 2
	
	var panning = Vector2()
	if Input.is_action_pressed("view_pan_up"):
		panning.y -= speed
	if Input.is_action_pressed("view_pan_down"):
		panning.y += speed
	if Input.is_action_pressed("view_pan_left"):
		panning.x -= speed
	if Input.is_action_pressed("view_pan_right"):
		panning.x += speed
	
	if panning.length_squared() > 0:
		offset += panning * pan_speed * delta * zoom
	
	if not offset.is_equal_approx(last_position):
		emit_signal("camera_moved")
	last_position = offset

func zoom_camera(zoom_factor, mouse_position):
	var viewport_size = get_viewport().size
	var previous_zoom = zoom
	zoom += zoom * zoom_factor
	zoom.x = max(zoom.x, min_zoom)
	zoom.y = max(zoom.y, min_zoom)
	offset += ((viewport_size * 0.5) - mouse_position) * (zoom-previous_zoom)
