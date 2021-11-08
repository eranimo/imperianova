shader_type canvas_item;
// FROM: https://wiki.orx-project.org/en/tutorials/shaders/hexagongrid2

uniform float hexSize;
uniform vec2 highlight;
uniform vec2 gridSize;
uniform vec2 containerSize;
uniform sampler2D hexColors;
uniform float gridThickness;

const float PI = 3.14159265358979323846;
 
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
		vec2(1, 0), vec2(1, -1), vec2(0, -1),
		vec2(-1, 0), vec2(-1, 1), vec2(0, 1)
	};
	return directions[direction];
}

vec2 axial_add(vec2 hex, vec2 vec) {
	return vec2(hex.x + vec.x, hex.y + vec.x);
}

vec2 axial_neighbor(vec2 hex, int direction) {
	return axial_add(hex, axial_direction(direction));
}

float distance_to_line(vec2 pt1, vec2 pt2, vec2 testPt) {
  vec2 lineDir = pt2 - pt1;
  vec2 perpDir = vec2(lineDir.y, -lineDir.x);
  vec2 dirToPt1 = pt1 - testPt;
  return abs(dot(normalize(perpDir), dirToPt1));
}

vec2 flat_hex_corner(vec2 center, int i) {
	float angle_deg = 60.0 * float(i);
	float angle_rad = PI / 180.0 * angle_deg;
	return vec2(
		center.x + hexSize * cos(angle_rad),
		center.y + hexSize * sin(angle_rad)
	);
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
		vec4 texel = texelFetch(hexColors, ivec2(int(offset.x), int(offset.y)), 0);
		color = vec4(texel.x, texel.y, texel.z, 1.0);
		
		if (highlight == hex) {
			color = mix(color, vec4(0.0, 0.0, 0.0, 1.0), 0.5);
		}

		// SE border
		for (int i = 0; i <= 5; i++) {
			int j;
			if (i == 5) {
				j = 0;
			} else {
				j = i + 1;
			}
			vec2 SE_n = axial_neighbor(hex, i);
			vec2 SE_p1 = flat_hex_corner(center, i);
			vec2 SE_p2 = flat_hex_corner(center, j);
			float SE_d = distance_to_line(SE_p1, SE_p2, point);
			if (SE_d < gridThickness) {
				color = mix(color, vec4(0, 0, 0, 1), smoothstep(gridThickness, 0.0, SE_d));
			}
		}
	
	}


	COLOR = color;
}
