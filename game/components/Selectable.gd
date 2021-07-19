extends CollisionShape2D
class_name Selectable

const component_name = 'Selectable'

var is_selected = false setget set_selected

func _ready():
	EntitySystem.register_component(self)

func set_selected(value):
	is_selected = value
	print("Set selected ", value)
	$SelectionSprite.visible = is_selected
	
	if EntitySystem.has_component(self, "Movement"):
		EntitySystem.get_component(self, "Movement").path_visible = is_selected
		update()
