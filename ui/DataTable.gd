extends Control
class_name DataTable

var root: TreeItem

export var columns = []
var entity
var DataTree

func _ready():
	DataTree = get_node("DataTree")
	root = DataTree.create_item()
	DataTree.select_mode = Tree.SELECT_ROW
	DataTree.set_hide_root(true)
	DataTree.set_hide_folding(true)
	DataTree.set_column_titles_visible(true)
	
func set_columns(columns_):
	columns = columns_
	DataTree.columns = columns.size()
	for index in columns.size():
		DataTree.set_column_title(index, columns[index].label)

func add_item(item_data: Dictionary):
	var item = DataTree.create_item(root)
	for index in columns.size():
		var value = item_data.get(columns[index].key)
		item.set_text(index, str(value))
