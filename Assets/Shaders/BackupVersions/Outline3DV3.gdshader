shader_type spatial;
render_mode unshaded;

// Textures
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D NORMAL_TEXTURE : hint_normal_roughness_texture, filter_linear_mipmap;

// Outline Parameters
uniform vec3 outline_color: source_color = vec3(0.0, 0.0, 0.0);
uniform float outline_width : hint_range(1.0, 10.0, 0.5) = 1.0; // Width in pixels
uniform float normal_threshold : hint_range(0.0, 1.0, 0.01) = 0.1;
uniform float depth_threshold : hint_range(0.0, 0.5, 0.005) = 0.05;
// Optional: Controls how much depth threshold increases on surfaces facing away (reduces artifacts)
uniform float depth_artifact_correction_coef : hint_range(0.0, 10.0, 0.1) = 3.0;

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


// --- Helper Functions ---

// Modified: Takes sample_dist instead of pixel_size
float edge_value_normal(sampler2D normal_tex, vec2 uv, vec2 sample_dist, mat3 sobel) {
	float output = 0.0;
	// Use .rgb to explicitly get vec3, avoiding potential vec4 issues if format changes
	vec3 normal = texture(normal_tex, uv).rgb;
	// Clamp neighbour UVs to prevent reading outside texture edges, especially with larger sample_dist
	vec3 n  = texture(normal_tex, clamp(uv + vec2(0.0, -sample_dist.y), 0.0, 1.0)).rgb;
	vec3 s  = texture(normal_tex, clamp(uv + vec2(0.0,  sample_dist.y), 0.0, 1.0)).rgb;
	vec3 e  = texture(normal_tex, clamp(uv + vec2( sample_dist.x, 0.0), 0.0, 1.0)).rgb;
	vec3 w  = texture(normal_tex, clamp(uv + vec2(-sample_dist.x, 0.0), 0.0, 1.0)).rgb;
	vec3 nw = texture(normal_tex, clamp(uv + vec2(-sample_dist.x, -sample_dist.y), 0.0, 1.0)).rgb;
	vec3 ne = texture(normal_tex, clamp(uv + vec2( sample_dist.x, -sample_dist.y), 0.0, 1.0)).rgb;
	vec3 sw = texture(normal_tex, clamp(uv + vec2(-sample_dist.x,  sample_dist.y), 0.0, 1.0)).rgb;
	vec3 se = texture(normal_tex, clamp(uv + vec2( sample_dist.x,  sample_dist.y), 0.0, 1.0)).rgb;

	// Calculate difference magnitude between center and neighbours
	mat3 error_mat = mat3(
		vec3(distance(normal, nw), distance(normal, n), distance(normal, ne)),
		vec3(distance(normal, w),  0.0,                 distance(normal, e)),
		vec3(distance(normal, sw), distance(normal, s), distance(normal, se))
	);

	// Apply Sobel kernel using dot product
	output += dot(sobel[0], error_mat[0]);
	output += dot(sobel[1], error_mat[1]);
	output += dot(sobel[2], error_mat[2]);
	return abs(output);
}

// Gets linear depth (distance from camera) from raw depth buffer value
float get_depth(sampler2D depth_tex, vec2 uv, mat4 inv_projection_matrix) {
	// Clamp UV to prevent reading outside texture edges
	uv = clamp(uv, vec2(0.001), vec2(0.999)); // Small margin from edge
	float depth_raw = texture(depth_tex, uv).x;
	// If raw depth is 1.0 (or very close), it's background, return large depth
	if (depth_raw >= 0.9999) {
		return 10000.0; // Consistent large value for background
	}
	// Unproject foreground depth
	vec3 ndc = vec3(uv * 2.0 - 1.0, depth_raw);
	vec4 view = inv_projection_matrix * vec4(ndc, 1.0);
	// Handle potential division by zero/small w
	if (abs(view.w) < 0.0001) return 10000.0;
	view.xyz /= view.w;
	// Use positive distance
	return abs(view.z);
}

// Modified: Takes sample_dist instead of pixel_size
float edge_value_depth(sampler2D depth_tex, vec2 uv, vec2 sample_dist, mat3 sobel, mat4 inv_projection_matrix){
	float output = 0.0;
	float depth = get_depth(depth_tex, uv, inv_projection_matrix);
	// Avoid calculations if center depth is effectively infinite (background)
	if (depth > 9000.0) return 0.0;

	// Sample neighbours, clamping UVs
	float n  = get_depth(depth_tex, clamp(uv + vec2(0.0, -sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float s  = get_depth(depth_tex, clamp(uv + vec2(0.0,  sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float e  = get_depth(depth_tex, clamp(uv + vec2( sample_dist.x, 0.0), 0.0, 1.0), inv_projection_matrix);
	float w  = get_depth(depth_tex, clamp(uv + vec2(-sample_dist.x, 0.0), 0.0, 1.0), inv_projection_matrix);
	float ne = get_depth(depth_tex, clamp(uv + vec2( sample_dist.x, -sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float nw = get_depth(depth_tex, clamp(uv + vec2(-sample_dist.x, -sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float se = get_depth(depth_tex, clamp(uv + vec2( sample_dist.x,  sample_dist.y), 0.0, 1.0), inv_projection_matrix);
	float sw = get_depth(depth_tex, clamp(uv + vec2(-sample_dist.x,  sample_dist.y), 0.0, 1.0), inv_projection_matrix);

	// Calculate relative difference magnitude (prevents huge values for distant objects)
	// Avoid division by zero if depth is very small
	float inv_depth = 1.0 / max(depth, 0.001);
	mat3 error_mat = mat3(
		vec3(abs(depth - nw)*inv_depth, abs(depth - n)*inv_depth, abs(depth - ne)*inv_depth),
		vec3(abs(depth - w) *inv_depth, 0.0,                   abs(depth - e)*inv_depth),
		vec3(abs(depth - sw)*inv_depth, abs(depth - s)*inv_depth, abs(depth - se)*inv_depth)
	);

	// Apply Sobel kernel
	output += dot(sobel[0], error_mat[0]);
	output += dot(sobel[1], error_mat[1]);
	output += dot(sobel[2], error_mat[2]);
	return abs(output);
}

// --- Vertex Shader ---
void vertex() {
	// Standard full-screen quad setup
	POSITION = vec4(VERTEX.xy, 0.0, 1.0);
}

// --- Fragment Shader (Modified for Thickness) ---
void fragment() {
	// Calculate base pixel size for 1 pixel offset
	vec2 pixel_size = vec2(1.0) / VIEWPORT_SIZE;
	// Get current pixel's screen UV
	vec2 uv = SCREEN_UV;

	// Start with the original screen texture color
	ALBEDO = texture(SCREEN_TEXTURE, uv).rgb;

	// Calculate angle factor for depth artifact correction (once per pixel)
	// Use .rgb to be explicit, decode normal if necessary (depends on texture format, assume standard view-space normal map)
	vec3 view_normal = texture(NORMAL_TEXTURE, uv).rgb * 2.0 - 1.0; // Assuming encoded normal needs unpack
	// Or if normal texture already stores view-space directly: vec3 view_normal = texture(NORMAL_TEXTURE, uv).rgb;
	float angle_factor = 1.0 - dot(normalize(view_normal), vec3(0.0, 0.0, 1.0)); // Angle wrt view Z axis

	// Loop through sample distances based on outline_width
	// Add a small epsilon to outline_width check to handle float inaccuracies
	for (float i = 1.0; i <= floor(outline_width + 0.001); i += 1.0) {
		// Calculate the sample distance for this iteration
		vec2 current_sample_dist = pixel_size * i;

		// --- Normal Edge Check ---
		float normal_edge_x = edge_value_normal(NORMAL_TEXTURE, uv, current_sample_dist, sobel_x);
		float normal_edge_y = edge_value_normal(NORMAL_TEXTURE, uv, current_sample_dist, sobel_y);
		// Combine X and Y Sobel results (magnitude approximation)
		float total_normal_edge = normal_edge_x + normal_edge_y;

		// --- Depth Edge Check ---
		float depth_edge_x = edge_value_depth(DEPTH_TEXTURE, uv, current_sample_dist, sobel_x, INV_PROJECTION_MATRIX);
		float depth_edge_y = edge_value_depth(DEPTH_TEXTURE, uv, current_sample_dist, sobel_y, INV_PROJECTION_MATRIX);
		// Combine X and Y Sobel results
		float total_depth_edge = depth_edge_x + depth_edge_y;

		// --- Apply Outline Color if Edge Detected ---
		bool normal_edge_found = total_normal_edge > normal_threshold;
		// Apply angle correction to depth threshold
		bool depth_edge_found = total_depth_edge > (depth_threshold + angle_factor * depth_artifact_correction_coef);

		if (normal_edge_found || depth_edge_found) {
			// Edge found at this distance, apply color and exit loop
			ALBEDO = outline_color;
			break;
		}
	}
	// If no edge was found within the loop, ALBEDO remains the original screen_color
}
