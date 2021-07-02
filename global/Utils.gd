extends Node

class Memoize:
	var _cache
	var _obj
	var _func_name
	
	func _init(obj: Object, func_name: String):
		_cache = {}
		_obj = obj
		_func_name = func_name
	
	func call(arguments):
		if _cache.has