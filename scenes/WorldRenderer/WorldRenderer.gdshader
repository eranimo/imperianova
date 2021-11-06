shader_type canvas_item;
// FROM: https://wiki.orx-project.org/en/tutorials/shaders/hexagongrid2

uniform float hexSize;
uniform vec2 highlight;
uniform vec2 gridSize;
uniform sampler2D hexColors;
 
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
 
// Main shader.
void fragment() {
	//vec2 point = vec2(FRAGCOORD.x, FRAGCOORD.y);
	vec2 point = UV * 500.0 + vec2(-hexSize, -hexSize);
	vec2 hex = axialRound(pixelToHex_FlatTop(point));
	vec2 offset = axialToOffset(hex);
 
	float width = 2.0 * hexSize;
	float height = sqrt(3.0) * hexSize;
 
	vec2 center = hexToPixel_FlatTop(hex);
	vec2 origin = vec2(center.x - (width/2.0), center.y - (height/2.0));

	vec2 textureCoord = vec2(((point.x - origin.x) / width), 1.0 - ((point.y - origin.y) / height));
	
    vec4 color;
    if (offset.x < 0.0 || offset.y < 0.0 || offset.x >= gridSize.x || offset.y >= gridSize.y) {
		color = vec4(0.0, 0.0, 0.0, 0.0)
	} else {
        vec4 texel = texelFetch(hexColors, ivec2(int(offset.x), int(offset.y)), 0);
        color = vec4(texel.x, texel.y, texel.z, 1.0);
        
        if (highlight == hex) {
            color = mix(color, vec4(0.0, 0.0, 0.0, 1.0), 0.5);
        }
    }
 
	COLOR = color;
}
