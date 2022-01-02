shader_type spatial;
uniform sampler2D gridTexture;

void fragment(){
    vec2 gridUV = UV - vec2(20.78460969, 20.78460969);
    gridUV.x *= 1.0 / (3.0 * 24.0);
    gridUV.y *= 1.0 / (4.0 * 20.78460969);
    vec4 grid = texture(gridTexture, gridUV);
    ALBEDO = vec3(COLOR[0], COLOR[1], COLOR[2]) * grid.xyz;
}