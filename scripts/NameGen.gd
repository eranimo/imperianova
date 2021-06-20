extends Object
class_name NameGen

var alphabet = ['a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z']

var markov = {}

func load_names(names):
	for name in names:
		var currName = name
		for i in range(currName.length()):
			var currLetter = currName[i].to_lower()
			var letterToAdd;
			if i == (currName.length() - 1):
				letterToAdd = "."
			else:
				letterToAdd = currName[i+1]
			var tempList = []
			if markov.has(currLetter):
				tempList = markov[currLetter]
			tempList.append(letterToAdd)
			markov[currLetter] = tempList
	return self

func add_from_file(name_list):
	var file = File.new()
	file.open('res://resources/name_lists/%s.txt' % name_list, File.READ)
	var names = []
	while not file.eof_reached():
		names.append(file.get_line())
	load_names(names)
	return self

func _get_next_letter(letter):
	var thisList = markov[letter]
	return thisList[roll(0, thisList.size()-1)]

func generate_name(minLength = 4, maxLength = 7):
	var count = 1
	var name = markov.keys()[roll(0, alphabet.size()-1)]
	while count < maxLength:
		var new_last = name.length()-1
		var nextLetter = _get_next_letter(name[new_last])
		if str(nextLetter) == ".":
			if count > minLength:
				return name
		else:
			name += str(nextLetter)
			count += 1
	return name.capitalize()

func generate_names(count, minLength = 4, maxLength = 7):
	var names = []
	for _i in range(count):
		names.append(generate_name(minLength, maxLength))
	return names

func roll(l,h):
	return int(round(rand_range(l,h)))
