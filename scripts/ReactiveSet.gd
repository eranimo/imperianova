extends Object
class_name ReactiveSet

var data = {}
var subscriptions = {}

func subscribe(obj: Object, method: String):
	subscriptions[obj] = method
	_notify()

func unsubscribe(obj: Object):
	subscriptions.erase(obj)

func _notify():
	for obj in subscriptions:
		var method = subscriptions[obj]
		obj.call(method, data.keys())

func add(value):
	data[value] = true
	_notify()

func remove(value):
	data.erase(value)
	_notify()

func clear():
	data.clear()
	_notify()


