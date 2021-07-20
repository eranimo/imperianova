extends ScrollContainer

onready var UnitPanelContainer: Node = find_node("UnitPanelContainer")
onready var UnitPanel = preload("res://ui/UnitPanel.tscn")

func _ready():
	MapManager.selected_units.subscribe(self, "_on_selected_units_update")


func _on_selected_units_update(selected_units):
	for unit_panel in UnitPanelContainer.get_children():
		UnitPanelContainer.remove_child(unit_panel)
	for unit in selected_units:
		var unit_panel = UnitPanel.instance()
		unit_panel.unit = unit
		unit_panel.connect("deselect", self, "_unit_panel_deselect", [unit])
		
		UnitPanelContainer.add_child(unit_panel)

func _unit_panel_deselect(unit):
	MapManager.deselect_unit(unit)
