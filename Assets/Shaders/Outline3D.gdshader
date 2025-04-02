shader_type spatial;
render_mode unshaded;

// Textures
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D NORMAL_TEXTURE : hint_normal_roughness_texture, filter_linear_mipmap;

// Outline Parameters
uniform float outline_intensity : hint_range(0.0, 1.0, 0.01) = 1.0; // Overall outline strength
uniform float outline_width : hint_range(0.0, 20.0, 0.5) = 5.0; // Width in pixels
uniform float normal_threshold : hint_range(0.0, 1.0, 0.01) = 0.1;
uniform float outline_colorblend_factor : hint_range(0.0, 1.0, 0.01) = 0.75; // Darkening parameter
uniform float alpha_threshold : hint_range(0.0, 1.0, 0.01) = 0.3; // Threshold for considering a pixel non-transparent
uniform vec3 outline_fallback_color : source_color = vec3(0.0, 0.0, 0.0); // Fallback color for outline

//We can make these Uniforms is required.
//uniform float outline_depth_threshold : hint_range(0.0, 0.5, 0.005) = 0.05;
//uniform float outline_depth_artifact_correction_coef : hint_range(0.0, 10.0, 0.1) = 3.0;
const float outline_depth_threshold = 0.05;
const float outline_depth_artifact_correction_coef = 3.0;


// Sobel Kernels (Constants)
const mat3 sobel_y = mat3(
	vec3(1.0, 0.0, -1.0),
	vec3(2.0, 0.0, -2.0),
	vec3(1.0, 0.0, -1.0)
);
const mat3 sobel_x = mat3(
	vec3(1.0, 2.0, 1.0),
	vec3(0.0, 0.0, 0.0),
	vec3(-1.0, -2.0, -1.0)
);

// Helper function to darken a color
vec3 darken_color(vec3 color, float factor) {
	return mix(color, outline_fallback_color, factor);
}

// Helper function to check if a pixel is valid (not fully transparent)
bool is_valid_pixel(vec4 pixel_color) {
	return pixel_color.a > alpha_threshold;
}

// Helper Functions (same as previous shader)
float edge_value_normal(sampler2D normal_tex, vec2 uv, vec2 sample_dist, mat3 sobel) {
	float output = 0.0;
	vec3 normal = texture(normal_tex, uv).rgb;
	vec3 n  = texture(normal_tex, clamp(uv + vec2(0.0, -sample_dist.y), 0.0, 1.0)).rgb;
	vec3 s  = texture(normal_tex, clamp(uv + vec2(0.0,  sample_dist.y), 0.0, 1.0)).rgb;
	vec3 e  = texture(normal_tex, clamp(uv + vec2( sample_dist.x, 0.0), 0.0, 1.0)).rgb;
	vec3 w  = texture(normal_tex, clamp(uv + vec2(-sample_dist.x, 0.0), 0.0, 1.0)).rgb;
	vec3 nw = texture(normal_tex, clamp(uv + vec2(-sample_dist.x, -sample_dist.y), 0.0, 1.0)).rgb;
	vec3 ne = texture(normal_tex, clamp(uv + vec2( sample_dist.x, -sample_dist.y), 0.0, 1.0)).rgb;
	vec3 sw = texture(normal_tex, clamp(uv + vec2(-sample_dist.x,  sample_dist.y), 0.0, 1.0)).rgb;
	vec3 se = texture(normal_tex, clamp(uv + vec2( sample_dist.x,  sample_dist.y), 0.0, 1.0)).rgb;

	mat3 error_mat = mat3(
		vec3(distance(normal, nw), distance(normal, n), distance(normal, ne)),
		vec3(distance(normal, w),  0.0,                 distance(normal, e)),
		vec3(distance(normal, sw), distance(normal, s), distance(normal, se))
	);

	output += dot(sobel[0], error_mat[0]);
	output += dot(sobel[1], error_mat[1]);
	output += dot(sobel[2], error_mat[2]);
	return abs(output);
}

float get_depth(sampler2D depth_tex, vec2 uv, mat4 inv_projection_matrix) {
	uv = clamp(uv, vec2(0.001), vec2(0.999));
	float depth_raw = texture(depth_tex, uv).x;
	if (depth_raw >= 0.9999) {
		return 10000.0;
	}
	vec3 ndc = vec3(uv * 2.0 - 1.0, depth_raw);
	vec4 view = inv_projection_matrix * vec4(ndc, 1.0);
	if (abs(view.w) < 0.0001) return 10000.0;
	view.xyz /= view.w;
	return abs(view.z);
}

float edge_value_depth(sampler2D depth_tex, vec2 uv, vec2 sample_dist, mat3 sobel, mat4 inv_projection_matrix){
	float output = 0.0;
	float depth = get_depth(depth_tex, uv, inv_projection_matrix);
	if (depth > 9000.0) return 0.0;

	float n  = get_depth(depth_tex, clamp(uv + vec2(0.0, -sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float s  = get_depth(depth_tex, clamp(uv + vec2(0.0,  sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float e  = get_depth(depth_tex, clamp(uv + vec2( sample_dist.x, 0.0), 0.0, 1.0), inv_projection_matrix);
	float w  = get_depth(depth_tex, clamp(uv + vec2(-sample_dist.x, 0.0), 0.0, 1.0), inv_projection_matrix);
	float ne = get_depth(depth_tex, clamp(uv + vec2( sample_dist.x, -sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float nw = get_depth(depth_tex, clamp(uv + vec2(-sample_dist.x, -sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float se = get_depth(depth_tex, clamp(uv + vec2( sample_dist.x,  sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float sw = get_depth(depth_tex, clamp(uv + vec2(-sample_dist.x,  sample_dist.y), 0.0, 1.0), inv_projection_matrix);

	float inv_depth = 1.0 / max(depth, 0.001);
	mat3 error_mat = mat3(
		vec3(abs(depth - nw)*inv_depth, abs(depth - n)*inv_depth, abs(depth - ne)*inv_depth),
		vec3(abs(depth - w) *inv_depth, 0.0,                   abs(depth - e)*inv_depth),
		vec3(abs(depth - sw)*inv_depth, abs(depth - s)*inv_depth, abs(depth - se)*inv_depth)
	);

	output += dot(sobel[0], error_mat[0]);
	output += dot(sobel[1], error_mat[1]);
	output += dot(sobel[2], error_mat[2]);
	return abs(output);
}

// Vertex Shader
void vertex() {
	POSITION = vec4(VERTEX.xy, 0.0, 1.0);
}

// Fragment Shader
void fragment() {
	vec2 pixel_size = vec2(1.0) / VIEWPORT_SIZE;
	vec2 uv = SCREEN_UV;

	// Original screen color
	vec4 screen_color_full = texture(SCREEN_TEXTURE, uv);
	ALBEDO = screen_color_full.rgb;

	// Calculate angle factor for depth artifact correction
	vec3 view_normal = texture(NORMAL_TEXTURE, uv).rgb * 2.0 - 1.0;
	float angle_factor = 1.0 - dot(normalize(view_normal), vec3(0.0, 0.0, 1.0));

	// Variables to track edge color and detection
	vec3 edge_color = vec3(0.0);
	float max_edge_value = 0.0;
	bool edge_found = false;

	// Loop through sample distances based on outline_width
	for (float i = 1.0; i <= floor(outline_width + 0.001); i += 1.0) {
		vec2 current_sample_dist = pixel_size * i;

		// Normal Edge Check
		float normal_edge_x = edge_value_normal(NORMAL_TEXTURE, uv, current_sample_dist, sobel_x);
		float normal_edge_y = edge_value_normal(NORMAL_TEXTURE, uv, current_sample_dist, sobel_y);
		float total_normal_edge = normal_edge_x + normal_edge_y;

		// Depth Edge Check
		float depth_edge_x = edge_value_depth(DEPTH_TEXTURE, uv, current_sample_dist, sobel_x, INV_PROJECTION_MATRIX);
		float depth_edge_y = edge_value_depth(DEPTH_TEXTURE, uv, current_sample_dist, sobel_y, INV_PROJECTION_MATRIX);
		float total_depth_edge = depth_edge_x + depth_edge_y;

		// Check for edges with intensity tracking
		bool normal_edge_found = total_normal_edge > normal_threshold;
		bool depth_edge_found = total_depth_edge > (outline_depth_threshold + angle_factor * outline_depth_artifact_correction_coef);

		if (normal_edge_found || depth_edge_found) {
			// Search for a valid pixel color in multiple directions
			vec2 search_directions[4] = vec2[](
				vec2( current_sample_dist.x,  0.0),
				vec2(-current_sample_dist.x,  0.0),
				vec2( 0.0,  current_sample_dist.y),
				vec2( 0.0, -current_sample_dist.y)
			);

			vec3 best_edge_color = vec3(0.0);
			float best_edge_value = 0.0;

			// Try multiple directions to find a valid pixel color
			for (int j = 0; j < 4; j++) {
				vec2 edge_uv = uv + search_directions[j];
				vec4 current_edge_pixel = texture(SCREEN_TEXTURE, clamp(edge_uv, 0.0, 1.0));

				// Check if the pixel is valid (not transparent)
				if (is_valid_pixel(current_edge_pixel)) {
					float current_edge_value = max(total_normal_edge, total_depth_edge);
					if (current_edge_value > best_edge_value) {
						best_edge_value = current_edge_value;
						best_edge_color = current_edge_pixel.rgb;
						edge_found = true;
					}
				}
			}

			// Update edge color if a valid pixel was found
			if (edge_found) {
				edge_color = best_edge_color;
				max_edge_value = best_edge_value;
			}
		}
	}

	// If an edge is found, apply darkening and blending
	if (edge_found) {
		// Darken the edge color
		vec3 darkened_edge_color = darken_color(edge_color, outline_colorblend_factor);

		// Blend based on outline intensity and edge detection
		ALBEDO = mix(screen_color_full.rgb, darkened_edge_color, outline_intensity);
	}
}