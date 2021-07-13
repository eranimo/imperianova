extends Camera2D

signal camera_moved

var zoom_step = 0.1
var min_zoom = 0.5
var max_zoom = 2.0

var pan_speed = 600
var last_position = Vector2()

var _mouse_captured = false

func _unhandled_input(event):
	print(event.as_text())
	# mouse wheel zoom
	if event is InputEventMouseButton:
		if event.is_action_pressed("view_zoom_in"):
			zoom /= 1 + zoom_step
		if event.is_action_pressed("view_zoom_out"):
			zoom *= 1 + zoom_step

	# trackpad pinch zoom
	if event is InputEventMagnifyGesture:
		if event.factor < 1:
			zoom /= 1 + (zoom_step / 3)
		else:
			zoom *= 1 + (zoom_step / 3)
	
	# trackpad panning
	if event is InputEventPanGesture:
		position += event.delta * 25
	
	# mouse middle click panning
	if event.is_action_pressed("view_pan_mouse"):
		_mouse_captured = true
	elif event.is_action_released("view_pan_mouse"):
		_mouse_captured = false
	if _mouse_captured && event is InputEventMouseMotion:
		position -= event.relative * zoom

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
		position += panning * pan_speed * delta * zoom
	
	if not position.is_equal_approx(last_position):
		emit_signal("camera_moved")
	last_position = position
