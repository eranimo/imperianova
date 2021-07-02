tool
extends HBoxContainer

export(String) var label setget _set_label
export var value = '' setget _set_value

func _set_label(label_):
	label = label_
	$Label.text = "%s:" % str(label_)

func _set_value(value_):
	value = value_
	$Value.text = str(value_)
