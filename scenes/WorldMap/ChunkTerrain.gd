extends SectionalTilemap

# Draws the center of a tile
func _draw_tile_center(
	_tile_pos: Vector2,
	dest: Rect2,
	tile_data
):
	var base_image = MapTilesets.terrain_type_base_tileset[tile_data.terrain_type]
	var src_rect = MapTilesets.get_tile_rect(0, 7)
	draw_texture_rect_region(base_image, dest, src_rect)

func _draw_tile_edge(
	tile_pos: Vector2,
	dest: Rect2,
	tile_data,
	section
):
	var terrain_type = tile_data.terrain_type
	var section_column_id = MapTilesets.base_column_ids[section]
	var dir = MapData.section_to_direction[section]
	var base_image = MapTilesets.terrain_type_base_tileset[terrain_type]
	var adj_dir_1 = MapData.direction_clockwise[dir]
	var adj_dir_2 = MapData.direction_counter_clockwise[dir]
	var edge_terrain = MapData.get_neighbor_tile(tile_pos, dir).terrain_type
	var adj1_terrain = MapData.get_neighbor_tile(tile_pos, adj_dir_1).terrain_type
	var adj2_terrain = MapData.get_neighbor_tile(tile_pos, adj_dir_2).terrain_type
	
	var has_trans_edge = MapData.has_transition(terrain_type, edge_terrain)
	var has_trans_adj1 = MapData.has_transition(terrain_type, adj1_terrain)
	var has_trans_adj2 = MapData.has_transition(terrain_type, adj2_terrain)

	var source_img = null
	var rect = null
	if has_trans_edge:
		if has_trans_adj2 and adj1_terrain != edge_terrain and adj2_terrain == edge_terrain:
			if MapData.has_transition(edge_terrain, adj1_terrain):
				# 3-trans Row 6
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(6, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain, adj1_terrain)
			else:
				# Row 4: edge = secondary; adj1 = primary; adj2 = secondary
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(4, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain)
		elif has_trans_adj1 and adj1_terrain == edge_terrain and adj2_terrain != edge_terrain:
			if MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 7
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(7, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain, adj2_terrain)
			else:
				# Row 5: edge = secondary; adj1 = secondary; adj2 = primary
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(5, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain)
		elif has_trans_adj1 and has_trans_adj2 and adj1_terrain == edge_terrain and adj2_terrain == edge_terrain:
			# Row 6: edge = secondary; adj1 = secondary; adj2 = secondary
			rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(6, section), 6)
			source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain)
		elif adj1_terrain != edge_terrain and adj2_terrain != edge_terrain:
			if MapData.has_transition(edge_terrain, adj1_terrain) and not MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 1
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(1, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain, adj1_terrain)
			elif not MapData.has_transition(edge_terrain, adj1_terrain) and MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 2
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(2, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain, adj2_terrain)
			elif MapData.has_transition(edge_terrain, adj1_terrain) and MapData.has_transition(edge_terrain, adj2_terrain):
				# 3-trans Row 3
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(3, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain, adj1_terrain)
			else:
				# Row 7: edge = secondary; adj1 = primary; adj2 = primary
				rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(7, section), 6)
				source_img = MapTilesets._get_transition_image(terrain_type, edge_terrain)
	elif has_trans_adj1 or has_trans_adj2:
		if has_trans_adj1 and adj2_terrain != adj1_terrain:
			# Row 1: edge = primary; adj1 = secondary; adj2 = primary
			rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(1, section), 6)
			source_img = MapTilesets._get_transition_image(terrain_type, adj1_terrain)
		elif has_trans_adj2 and adj1_terrain != adj2_terrain:
			# Row 2: edge = primary; adj1 = primary; adj2 = secondary
			rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(2, section), 6)
			source_img = MapTilesets._get_transition_image(terrain_type, adj2_terrain)
		elif has_trans_adj1 and has_trans_adj2:
			# Row 3: edge = primary; adj1, adj2 = secondary
			rect = MapTilesets.get_tile_rect(MapTilesets.get_transtion_tile_id(3, section), 6)
			source_img = MapTilesets._get_transition_image(terrain_type, adj1_terrain)
	else:
		source_img = base_image
		rect = MapTilesets.get_tile_rect(section_column_id, 7)

	draw_texture_rect_region(source_img, dest, rect)
