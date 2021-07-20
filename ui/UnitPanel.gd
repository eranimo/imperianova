extends Panel

signal deselect

onready var UnitName: Label = find_node("UnitName")
var unit setget set_unit

func set_unit(value):
	unit = value


func _on_CloseButton_pressed():
	emit_signal("deselect")
