extends Node
class_name DataTableColumn

export(String) var label
export(String) var key


func _ready():
	get_parent().add_column(self)
