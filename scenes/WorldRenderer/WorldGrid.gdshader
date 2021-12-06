shader_type canvas_item;
// FROM: https://wiki.orx-project.org/en/tutorials/shaders/hexagongrid2

uniform float hexSize;
uniform vec2 highlight;
uniform vec2 selectedHex;
uniform vec2 gridSize;
uniform vec2 containerSize;
uniform sampler2D hexColors;
uniform float gridThickness = 0.5;
uniform bool gridVisible;
uniform sampler2D hexTerritoryColor;
uniform bool territoriesVisible = true;
uniform float territoryBorderThickness = 1.0;
uniform float territoryGradientThickness = 6.0;
uniform float territoryGradientOpacity = 0.6;
uniform float gridLineOpacity = 0.75;
uniform sampler2D hexOccupiedColor;
uniform float zoom;
uniform sampler2D hexArrowAngles;
uniform bool showHexArrows;

const float PI = 3.14159265358979323846;
const float SQRT_2 = 1.4142135623730951;
const float arrow_linewidth = 2.0;
const float arrow_antialias =  1.0;
 
// Function to convert cubial coords to axial coords.
vec2 cubeToAxial(vec3 cube) {
	return vec2(cube.x, cube.y);
}
 
// Function to convert axial coords to cubial coords.
vec3 axialToCube(vec2 axial) {
	float x = axial.x;
	float y = axial.y;
 
	float z = -x - y;
 
	return vec3(x, y, z);
}

vec2 axialToOffset(vec2 hex) {
	float col = hex.x;
	float row = hex.y + (hex.x - (float(int(hex.x) & 1))) / 2.0;
	return vec2(col, row);
}

vec2 offsetToAxial(vec2 hex) {
	float q = hex.y;
	float r = hex.x - (hex.y - (float(int(hex.y) & 1))) / 2.0;
	return vec2(q, r);
}
 
// Function to round float cubial coords to int cubial coords.
vec3 cubeRound(vec3 cube) {
	float rx = round(cube.x);
	float ry = round(cube.y);
	float rz = round(cube.z);
 
	float xDiff = abs(rx - cube.x);
	float yDiff = abs(ry - cube.y);
	float zDiff = abs(rz - cube.z);
 
	if ((xDiff > yDiff) && (xDiff > zDiff))
		rx = -ry - rz;
 
	else if (yDiff > zDiff)
		ry = -rx - rz;
 
	else
		rz = -rx - ry;
 
	return vec3(rx, ry, rz);
}
 
// Function to round float axial coords to int axial coords.
vec2 axialRound(vec2 axial) {
	return cubeToAxial(cubeRound(axialToCube(axial)));
}
 
// Function to return axial hex-grid coords, given a screen position (vertical, flat-top hex layout).
vec2 pixelToHex_FlatTop(vec2 point) {
	return vec2((2.0/3.0 * point.x) / hexSize, ((-1.0/3.0 * point.x) + (sqrt(3.0)/3.0 * point.y)) / hexSize);
}
 
// Function to return a screen position, given axial hex-grid coords (vertical, flat-top hex layout).
vec2 hexToPixel_FlatTop(vec2 hex) {
	return vec2((3.0/2.0 * hex.x) * hexSize, ((sqrt(3.0)/2.0 * hex.x) + (sqrt(3.0) * hex.y)) * hexSize);
}

vec2 axial_direction(int direction) {
	vec2 directions[] = {
		vec2(1, 0), vec2(0, 1), vec2(-1, 1),
		vec2(-1, 0), vec2(0, -1), vec2(1, -1)
	};
	return directions[direction];
}

vec2 axial_neighbor(vec2 hex, int direction) {
	return hex + axial_direction(direction);
}

float distance_to_line(vec2 pt1, vec2 pt2, vec2 testPt) {
  vec2 lineDir = pt2 - pt1;
  vec2 perpDir = vec2(lineDir.y, -lineDir.x);
  vec2 dirToPt1 = pt1 - testPt;
  return abs(dot(normalize(perpDir), dirToPt1));
}

vec2 flat_hex_corner(vec2 center, int i, float hexSizeMod) {
	float angle_deg = 60.0 * float(i);
	float angle_rad = PI / 180.0 * angle_deg;
	return vec2(
		center.x + (hexSize - hexSizeMod) * cos(angle_rad),
		center.y + (hexSize - hexSizeMod) * sin(angle_rad)
	);
}


// ARROW DRAGING

vec4 filled_arrow(float distance, vec4 color, float linewidth, float antialias, vec4 fill){
	float t = linewidth/2.0 - antialias;
	float signed_distance = distance;
	float border_distance = abs(signed_distance) - t;
	float alpha = border_distance/antialias;
	alpha = exp(-alpha*alpha);

	// Within linestroke
	if(border_distance < 0.0)
		color = fill;
	// Within shape
	else if(signed_distance < 0.0)
		color = fill;
	else {
		// Outside shape
		if (border_distance <= (linewidth/2.0 + antialias)) {
			color = vec4(fill.rgb*alpha, 1.0);
		}
	}
	return color;
}

// Computes the signed distance from a line
float line_distance(vec2 p, vec2 p1, vec2 p2) {
	vec2 center = (p1 + p2) * 0.5;
	float len = length(p2 - p1);
	vec2 dir = (p2 - p1) / len;
	vec2 rel_p = p - center;
	return dot(rel_p, vec2(dir.y, -dir.x));
}

// Computes the signed distance from a line segment
float segment_distance(vec2 p, vec2 p1, vec2 p2) {
	vec2 center = (p1 + p2) * 0.5;
	float len = length(p2 - p1);
	vec2 dir = (p2 - p1) / len;
	vec2 rel_p = p - center;
	float dist1 = abs(dot(rel_p, vec2(dir.y, -dir.x)));
	float dist2 = abs(dot(rel_p, dir)) - 0.5*len;
	return max(dist1, dist2);
}


float arrow_triangle(
	vec2 texcoord,
	float body, float head, float height,
	float linewidth, float antialias
) {
	float w = linewidth/2.0 + antialias;
	vec2 start = -vec2(body/2.0, 0.0);
	vec2 end   = +vec2(body/2.0, 0.0);

	// Head : 3 lines
	float d1 = line_distance(texcoord, end, end - head*vec2(+1.0,-height));
	float d2 = line_distance(texcoord, end - head*vec2(+1.0,+height), end);
	float d3 = texcoord.x - end.x + head;

	// Body : 1 segment
	float d4 = segment_distance(texcoord, start, end - vec2(linewidth,0.0));

	float d = min(max(max(d1, d2), -d3), d4);
	return d;
}

float arrow_triangle_60(vec2 texcoord, float body, float head, float linewidth, float antialias) {
	return arrow_triangle(texcoord, body, head, 0.5, linewidth, antialias);
}

vec2 rotate(vec2 point, float degree, vec2 pivot) {
	float radAngle = -radians(degree);
	float x = point.x;
	float y = point.y;

	float rX = pivot.x + (x - pivot.x) * cos(radAngle) - (y - pivot.y) * sin(radAngle);
	float rY = pivot.y + (x - pivot.x) * sin(radAngle) + (y - pivot.y) * cos(radAngle);

	return vec2(rX, rY);
}

void fragment() {
	vec2 point = UV * containerSize - vec2((2.0 * hexSize) / 2.0, (sqrt(3.0) * hexSize) / 2.0);
	vec2 hex = axialRound(pixelToHex_FlatTop(point));
	vec2 offset = axialToOffset(hex);
 
	float width = 2.0 * hexSize;
	float height = sqrt(3.0) * hexSize;
 
	vec2 center = hexToPixel_FlatTop(hex);
	vec2 origin = vec2(center.x - (width/2.0), center.y - (height/2.0));

	vec2 textureCoord = vec2(((point.x - origin.x) / width), 1.0 - ((point.y - origin.y) / height));
	
	vec4 color;
	if (offset.x < 0.0 || offset.y < 0.0 || offset.x >= gridSize.x || offset.y >= gridSize.y) {
		color = vec4(0.0, 0.0, 0.0, 0.0);
	} else {
		// render hex color
		vec4 texel = texelFetch(hexColors, ivec2(int(offset.x), int(offset.y)), 0);
		color = vec4(texel.x, texel.y, texel.z, 1.0);
		
		// highlight rendering
		if (highlight == hex) {
			color = mix(color, vec4(0.0, 0.0, 0.0, 1.0), 0.1);
		}

		// occupied hex stripes
		vec4 occupiedColor = texelFetch(hexOccupiedColor, ivec2(int(offset.x), int(offset.y)), 0);
		if (occupiedColor != vec4(0)) {
			vec2 uv_scale = UV * 100.0;
			float pos = mix(uv_scale.x, uv_scale.y, 0.25) * 25.0;
			float value = floor(fract(pos) + 0.5);
			if (value == 1.0) {
				color = vec4(occupiedColor.xyz, 0.8);
			}
		}

		// territories
		vec4 territoryColor = texelFetch(hexTerritoryColor, ivec2(int(offset.x), int(offset.y)), 0);
		if (territoriesVisible) {
			if (territoryColor != vec4(0)) {
				for (int i = 0; i <= 5; i++) {
					int j;
					if (i == 5) {
						j = 0;
					} else {
						j = i + 1;
					}
					vec2 neighbor_axial = axial_neighbor(hex, i);
					vec2 p1 = flat_hex_corner(center, i, 0.0);
					vec2 p2 = flat_hex_corner(center, j, 0.0);
					float dist = distance_to_line(p1, p2, point);

					vec2 neighbor_offset = axialToOffset(neighbor_axial);
					vec4 neighborTerritoryColor = texelFetch(hexTerritoryColor, ivec2(int(neighbor_offset.x), int(neighbor_offset.y)), 0);
					bool is_same_territory = neighborTerritoryColor.a == territoryColor.a;

					if (!is_same_territory) {
						color = mix(color, vec4(territoryColor.xyz, territoryGradientOpacity), smoothstep(territoryGradientThickness, 0.0, dist));
					}
				}
			}
		}

		// grid lines rendering
		if (zoom < 1.5 && gridVisible) {
			for (int i = 0; i <= 5; i++) {
				int j;
				if (i == 5) {
					j = 0;
				} else {
					j = i + 1;
				}
				vec2 p1 = flat_hex_corner(center, i, 0.0);
				vec2 p2 = flat_hex_corner(center, j, 0.0);
				float dist = distance_to_line(p1, p2, point);

				vec4 borderColor = vec4(0, 0, 0, gridLineOpacity);
				float borderThickness = gridThickness;
				if (hex == selectedHex) {
					borderColor = vec4(1, 1, 1, 1);
					borderThickness = gridThickness * 2.0;
				}

				if (dist < borderThickness) {
					color = mix(color, borderColor, smoothstep(borderThickness, 0.0, dist));
				}
			}
		}

		// territory borders
		for (int i = 0; i <= 5; i++) {
			int j;
			if (i == 5) {
				j = 0;
			} else {
				j = i + 1;
			}
			vec2 neighbor_axial = axial_neighbor(hex, i);
			vec2 neighbor_offset = axialToOffset(neighbor_axial);
			vec4 neighborTerritoryColor = texelFetch(hexTerritoryColor, ivec2(int(neighbor_offset.x), int(neighbor_offset.y)), 0);
			bool is_same_territory = neighborTerritoryColor.a == territoryColor.a;

			if (!is_same_territory) {
				vec2 p1 = flat_hex_corner(center, i, 0.0);
				vec2 p2 = flat_hex_corner(center, j, 0.0);
				float dist = distance_to_line(p1, p2, point);
				if (neighborTerritoryColor == vec4(0)) {
					vec4 borderColor = territoryColor;
					if (dist < territoryBorderThickness) {
						color = mix(color, vec4(borderColor.xyz, 1.0), smoothstep(territoryBorderThickness, 0.0, dist));
					}
				} else {
					vec4 borderColor = neighborTerritoryColor;
					if (territoryColor != vec4(0)) {
						borderColor = territoryColor;
					}
					
					if (dist < territoryBorderThickness) {
						color = mix(color, vec4(borderColor.xyz, 1.0), smoothstep(territoryBorderThickness, 0.0, dist));
					}
				}
			}
		}

		// selection hex
		if (hex == selectedHex) {
			for (int i = 0; i <= 5; i++) {
				int j;
				if (i == 5) {
					j = 0;
				} else {
					j = i + 1;
				}
				vec2 p1 = flat_hex_corner(center, i, 0.0);
				vec2 p2 = flat_hex_corner(center, j, 0.0);
				float dist = distance_to_line(p1, p2, point);
				vec4 borderColor = vec4(1, 1, 1, 1);
				float borderThickness = 2.0;

				if (dist < borderThickness) {
					color = mix(color, borderColor, smoothstep(borderThickness, 0.0, dist));
				}
			}
		}

		// draw arrows
		if (showHexArrows) {
			float arrow_length = hexSize / 2.0;
			float arrow_angle = 128.0;
			float head = 3.0;
			float head_height = 3.0;
			vec2 end = rotate(center + vec2(arrow_length, 0), arrow_angle, center);
			vec2 h1 = rotate(center + vec2(arrow_length - head, head_height), arrow_angle, center);
			vec2 h2 = rotate(center + vec2(arrow_length - head, -head_height), arrow_angle, center);
			float dist = segment_distance(point, center, end);
			if (dist < 1.0) {
				color = vec4(1,1,1,1);
			}
			
			float d1 = segment_distance(point, end, h1);
			float d2 = segment_distance(point, h2, end);
			float d3 = point.x - end.x + head;
			// float dist_head = max(max(d1, d2), -d3);
			float dist_head = max(d1, d2);
			if (d1 < 1.0 || d2 < 1.0) {
				color = vec4(1,1,1,1);
			}
		}
	}


	COLOR = color;
}
