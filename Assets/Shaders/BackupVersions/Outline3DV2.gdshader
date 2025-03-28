shader_type spatial;
render_mode unshaded, fog_disabled;

// --- Uniforms (Same as before) ---
uniform float target_resolution : hint_range(32.0, 2048.0, 32.0) = 512.0;
uniform float outline_strength: hint_range(0.0, 1.0, 0.1) = 0.8;
uniform float outline_thickness: hint_range(0.1, 5.0, 0.1) = 1.0;
uniform vec4 outline_color : source_color = vec4(1.0, 0.0, 0.0, 1.0); // Red

group_uniforms outerline;
uniform float outerline_strength: hint_range(0.0, 1.0, 0.1) = 0.8;
uniform float outerline_thickness: hint_range(0.1, 10.0, 0.1) = 2.0;
uniform float outerline_offset: hint_range(0.0, 5.0, 0.1) = 1.0;
uniform vec4 outerline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0); // White

uniform sampler2D screen_texture: hint_screen_texture, filter_nearest;
uniform sampler2D depth_texture: hint_depth_texture, filter_nearest;
uniform sampler2D normal_texture: hint_normal_roughness_texture, filter_nearest;

// --- vertex() and helper functions (Same as before) ---
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

float calculate_depth_outline(float du, float dd, float dl, float dr, float dc, float thickness, float strength) {
	float h_depth = max(0.0, dl - dc) + max(0.0, dr - dc);
	float v_depth = max(0.0, du - dc) + max(0.0, dd - dc);

	float h_outline = step(0.1 * thickness, h_depth) * step(0.7, distance(dl, dr));
	float v_outline = step(0.1 * thickness, v_depth) * step(0.7, distance(du, dd));

	// Returns 1.0 if no outline, < 1.0 if outline detected
    return 1.0 - step(0.01, h_outline + v_outline) * strength;
}

// --- fragment() - Modified Combination Logic ---
void fragment() {
	vec2 uv = floor(SCREEN_UV * target_resolution) / target_resolution;
    vec2 pixel_size = vec2(1.0 / target_resolution);

	// --- Calculate Center Depth ---
	float dc_center = get_linear_depth(uv, INV_PROJECTION_MATRIX);

	// --- 1. Calculate INNER Outline ---
    vec2 uvs_inner[4] = {
        uv + vec2(0.0, pixel_size.y), uv - vec2(0.0, pixel_size.y),
        uv + vec2(pixel_size.x, 0.0), uv - vec2(pixel_size.x, 0.0)
    };
	float du_inner = get_linear_depth(uvs_inner[0], INV_PROJECTION_MATRIX);
	float dd_inner = get_linear_depth(uvs_inner[1], INV_PROJECTION_MATRIX);
	float dr_inner = get_linear_depth(uvs_inner[2], INV_PROJECTION_MATRIX);
	float dl_inner = get_linear_depth(uvs_inner[3], INV_PROJECTION_MATRIX);

	float depth_outline_factor = calculate_depth_outline(du_inner, dd_inner, dl_inner, dr_inner, dc_center, outline_thickness, outline_strength);
	float inner_mix_amount = 1.0 - depth_outline_factor; // 0 = no inner outline, >0 = inner outline

	// --- 2. Calculate OUTER Outline ---
	vec2 offset_dist = pixel_size * (1.0 + outerline_offset);
	vec2 uvs_outer[4] = {
        uv + vec2(0.0, offset_dist.y), uv - vec2(0.0, offset_dist.y),
        uv + vec2(offset_dist.x, 0.0), uv - vec2(offset_dist.x, 0.0)
    };
	float du_outer = get_linear_depth(uvs_outer[0], INV_PROJECTION_MATRIX);
	float dd_outer = get_linear_depth(uvs_outer[1], INV_PROJECTION_MATRIX);
	float dr_outer = get_linear_depth(uvs_outer[2], INV_PROJECTION_MATRIX);
	float dl_outer = get_linear_depth(uvs_outer[3], INV_PROJECTION_MATRIX);

	float outer_depth_outline_factor = calculate_depth_outline(du_outer, dd_outer, dl_outer, dr_outer, dc_center, outerline_thickness, outerline_strength);
	float outer_mix_amount = 1.0 - outer_depth_outline_factor; // 0 = no outer outline, >0 = outer outline

	// --- 3. Combine Results with Correct Priority ---
	vec3 color = texture(screen_texture, uv).rgb; // Get original screen color
	vec3 final_color = color; // Start with the original color

	// Use a small threshold to avoid floating point inaccuracies when comparing mix amounts
	float threshold = 0.01;

	if (inner_mix_amount > threshold) {
		// If the inner outline is detected, it ALWAYS takes priority.
		final_color = outline_color.rgb;
	} else if (outer_mix_amount > threshold) {
		// If the inner outline is NOT detected, BUT the outer outline IS,
		// then apply the outer outline color. This prevents overwriting the object pixels.
		final_color = outerline_color.rgb;
	}
	// else: // If neither inner nor outer outline is detected, final_color remains the original 'color'.

	// Assign final color
	ALBEDO = final_color;
	ALPHA = 1.0; // Keep opaque
}
