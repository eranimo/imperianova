extends Object
class_name EntityValue

var value = null
var subscriptions = {}

func _init(default_value = null):
	value = default_value

func subscribe(obj: Object, method: String, extra = []):
	subscriptions[obj] = [obj, method, extra]

func unsubscribe(obj: Object):
	subscriptions.erase(obj)

func _notify(old_value, new_value):
	for obj in subscriptions:
		var method = subscriptions[obj][1]
		var extra = subscriptions[obj][2]
		obj.call(method, new_value, old_value, extra)

func next(new_value):
	var old_value = value
	value = new_value
	_notify(old_value, new_value)
