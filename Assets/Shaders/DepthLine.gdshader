shader_type spatial;
render_mode unshaded, fog_disabled;

//DepthLine shader code
uniform float depthline_strength: hint_range(0.0, 1.0, 0.1) = 0.0;
uniform float  depthline_thickness: hint_range(0.1, 5.0, 0.1) = 1.0;
uniform vec4 depthline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

uniform sampler2D screen_texture: hint_screen_texture, filter_nearest;
uniform sampler2D depth_texture: hint_depth_texture, filter_nearest;
uniform sampler2D normal_texture: hint_normal_roughness_texture, filter_nearest;

void vertex() {
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

vec3 get_normal(vec2 uv) {
	return texture(normal_texture, uv).rgb;
}

float get_linear_depth(vec2 uv, mat4 inv_proj_mat) {
	float depth = texture(depth_texture, uv).r;
	vec3 ndc = vec3(uv * 2.0 - 1.0, depth);
	vec4 view = inv_proj_mat * vec4(ndc, 1.0);
	view.xyz /= view.w;
	return -view.z;
}

void fragment() {
	//vec2 uv = floor(SCREEN_UV * target_resolution) / target_resolution;
    //vec2 pixel_size = vec2(1.0 / target_resolution, 1.0 / target_resolution); // Corrected: Construct a vec2
	vec2 pixel_size = vec2(1.0) / VIEWPORT_SIZE;
	vec2 uv = SCREEN_UV;
	
	vec2 uvs[5] = {
        uv,
        uv + vec2(0.0, pixel_size.y), // Up
        uv - vec2(0.0, pixel_size.y), // Down
        uv + vec2(pixel_size.x, 0.0), // Left
        uv - vec2(pixel_size.x, 0.0)  // Right
    };

	float dc = get_linear_depth(uvs[0], INV_PROJECTION_MATRIX);
	float du = get_linear_depth(uvs[1], INV_PROJECTION_MATRIX);
	float dd = get_linear_depth(uvs[2], INV_PROJECTION_MATRIX);
	float dl = get_linear_depth(uvs[3], INV_PROJECTION_MATRIX);
	float dr = get_linear_depth(uvs[4], INV_PROJECTION_MATRIX);

    float h_depth = max(0.0, dl - dc) + max(0.0, dr - dc);
	float v_depth = max(0.0, du - dc) + max(0.0, dd - dc);

    // Adjust the outline detection threshold based on thickness
	float h_outline = step(0.1 * depthline_thickness, h_depth) * step(0.7, distance(dl, dr));
	float v_outline = step(0.1 * depthline_thickness, v_depth) * step(0.7, distance(du, dd));
    float depth_outline = 1.0 - step(0.01, h_outline + v_outline) * depthline_strength;

	vec3 color = texture(screen_texture, uv).rgb;
	ALBEDO = mix(color, depthline_color.rgb, 1.0 - depth_outline);
}
