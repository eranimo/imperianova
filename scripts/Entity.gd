extends Object
class_name Entity

var id: int
var data: Dictionary
var subscriptions = {}

func setup(data_):
	data = data_

func subscribe(obj: Object, method: String):
	subscriptions[obj] = [obj, method]
	_notify()

func unsubscribe(obj: Object):
	subscriptions.erase(obj)

func _notify():
	for obj in subscriptions:
		var method = subscriptions[obj][1]
		obj.call(method, data)

func set(key: String, value):
	data[key] = value
	_notify()

func get(key: String):
	return data[key]

func to_dict():
	assert(false, "to_dict() must be implemented")
