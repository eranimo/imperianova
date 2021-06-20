extends Object
class_name NameGen

var alphabet = ['a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z']

var markov = {}
var possible_first_letters = {}

func load_names(names):
	for name in names:
		possible_first_letters[name[0].to_lower()] = true
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
	if not markov.has(letter.to_lower()):
		return null
	var thisList = markov[letter.to_lower()]
	return thisList[roll(0, thisList.size()-1)]

func generate_name(minLength = 4, maxLength = 7):
	var count = 1
	var letter_list = possible_first_letters.keys()
	var name = letter_list[roll(0, letter_list.size()-1)].to_upper()
	while count < maxLength:
		var new_last = name.length()-1
		var nextLetter = _get_next_letter(name[new_last])
		if nextLetter == null:
			return generate_name(minLength, maxLength)
		if str(nextLetter) == ".":
			if count > minLength:
				return name
		else:
			name += str(nextLetter).to_lower()
			count += 1
	return name

func generate_names(count, minLength = 4, maxLength = 7):
	var names = []
	for _i in range(count):
		names.append(generate_name(minLength, maxLength))
	return names

func roll(l,h):
	return int(round(rand_range(l,h)))
