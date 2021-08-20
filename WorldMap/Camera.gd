extends Camera

var panning_speed = 1
var panning = false

func _input(event):
	if event.is_action_pressed("view_pan_mouse"):
		panning = true
	elif event.is_action_released("view_pan_mouse"):
		panning = false
	
	# Update
	if event is InputEventMouseMotion and panning == true:
		translation.x += event.relative.x * panning_speed;
		translation.z += event.relative.y * panning_speed;

	if event.is_action_released('view_zoom_in'):
		print(event.position)
		translation.y -= 100
	if event.is_action_released('view_zoom_out'):
		translation.y += 100
	translation.y = max(300, translation.y)
