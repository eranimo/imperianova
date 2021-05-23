extends Control


export(bool) var loading = false setget set_loading
export(bool) var loading_step = '' setget set_loading_step

onready var LoadingStep = $Inner/LoadingStep
onready var ProgressBar = $Inner/ProgressBar

func set_loading(loading_):
	visible = loading_

func set_loading_step(step):
	LoadingStep.text = step

# Called when the node enters the scene tree for the first time.
func _ready():
	loading = true
