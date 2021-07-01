class_name MapSet

var data

func _init():
	data = {}

func add(key, value):
	if not data.has(key):
		data[key] = {}
	data[key][value] = true
	
func has(key, value):
	if data.has(key):
		return data[key].has(value)
	else:
		return false

func get(key):
	return data[key]

func keys():
	return data.keys()

func values(key):
	if data.has(key):
		return data[key].keys()
	else:
		return []

func clear(key):
	if data.has(key):
		data[key] = {}
		return true
	else:
		return false

func remove(key, value):
	if data.has(key):
		data[key].erase(value)
	else:
		return false
