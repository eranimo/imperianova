extends Node

class PendingRef:
	var obj
	var field
	var ref_id
	
	func _init(obj, field, ref_id):
		self.obj = obj
		self.field = field
		self.ref_id = ref_id
	
var next_id = 0
var assignments = {}
var pending_refs = []

func reset():
	assignments.clear()
	next_id = 0

func assign_id(obj):
	assignments[next_id] = obj
	obj.save_id = next_id
	next_id += 1

func on_load(obj, id):
	assert(!assignments.has(id))
	obj.save_id = id
	next_id = max(next_id, id + 1)
	assignments[id] = obj

func get_obj(id):
	return assignments[id]

func pending_ref(obj, field, ref_id):
	pending_refs.append(PendingRef.new(obj, field, ref_id))

func resolve_refs():
	for ref in pending_refs:
		ref.obj.set(ref.field, get_obj(ref.ref_id))
	pending_refs.clear()
