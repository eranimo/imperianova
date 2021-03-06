extends Object
class_name ReactiveState

var value = null
var subscriptions = {}

func _init(default_value = null):
	value = default_value

func subscribe(obj: Object, method: String):
	subscriptions[obj] = [obj, method]
	_notify()

func unsubscribe(obj: Object):
	subscriptions.erase(obj)

func _notify():
	for obj in subscriptions:
		var method = subscriptions[obj][1]
		obj.call(method, value)

func next(new_value):
	value = new_value
	_notify()
