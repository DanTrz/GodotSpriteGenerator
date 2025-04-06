shader_type spatial;
// Render modes for spatial quad effect: unshaded, writes depth, needs depth texture access, renders flipped face
render_mode unshaded, depth_draw_opaque, depth_prepass_alpha, cull_front;

// Texture Inputs (Available to spatial shaders)
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D NORMAL_TEXTURE : hint_normal_roughness_texture, filter_nearest;

// Outline Appearance
uniform vec3 depthline_color : source_color = vec3(0.0); // Tint color for the outline
// New uniform to control the opacity of the shadow_color tint itself
uniform float colorblend_factor : hint_range(0.0, 1.0) = 0.75;
uniform float depthline_thickness : hint_range(0.0, 10.0) = 0.0;

// Sensitivity Controls
uniform float depth_sensitivity : hint_range(0.0, 10.0) = 0.0;
uniform float normal_sensitivity : hint_range(0.0, 5.0) = 0.00;
uniform float outline_threshold_min : hint_range(0.0, 1.0) = 0.2;
uniform float outline_threshold_max : hint_range(0.0, 1.0) = 0.3;

// Small depth difference threshold to consider a neighbor as "foreground"
uniform float foreground_depth_bias : hint_range(0.0, 0.5) = 0.01;


// Function to get linear view-space depth (-Z) and raw depth buffer value
vec2 getDepth(vec2 screen_uv, sampler2D depth_texture, mat4 inv_projection_matrix){
	screen_uv = clamp(screen_uv, vec2(0.0), vec2(1.0)); // Clamp UVs
	float raw_depth = texture(depth_texture, screen_uv)[0];
	// Check for maximum raw depth value (usually 1.0)
	if (raw_depth >= 1.0) return vec2(10000.0, 1.0); // Skybox/far plane depth

	vec3 normalized_device_coordinates = vec3(screen_uv * 2.0 - 1.0, raw_depth);
	vec4 view_space = inv_projection_matrix * vec4(normalized_device_coordinates, 1.0);
	// Check for perspective division by zero or near-zero
	if (abs(view_space.w) < 0.0001) return vec2(10000.0, raw_depth);
	view_space.xyz /= view_space.w;
	// Return linear depth (-Z, positive value) and raw depth
	return vec2(-view_space.z, raw_depth);
}


void fragment() {
	// Get UV and pixel size in screen space
	vec2 center_uv = SCREEN_UV;
	vec2 pixel_size = vec2(1.0 / VIEWPORT_SIZE.xy);

	// Get data for the current fragment (pixel)
	vec2 center_depth_data = getDepth(center_uv, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
	float center_depth = center_depth_data.x; // Linear view-space depth

	// --- Check if the current pixel corresponds to the skybox/far plane ---
	if (center_depth >= 9999.0) {
		// --- Handle Skybox/Far Plane Pixel ---
		// Make this part of the outline quad invisible by setting alpha to 0
		ALPHA = 0.0;
		// Assign default values - these won't be visible but must be set
		ALBEDO = vec3(0.0);
		DEPTH = center_depth_data.y; // Use the original raw depth (usually 1.0)

	} else {
		// --- Handle Pixels with Valid Depth (Perform Outline Calculation) ---

		// Accumulator for edge detection score
		float edge_score = 0.0;

		// Get center color and normal only if depth is valid
		vec3 center_color = texture(SCREEN_TEXTURE, center_uv).rgb;
		vec3 center_normal = texture(NORMAL_TEXTURE, center_uv).rgb * 2.0 - 1.0; // Unpack normal

		// --- Variables for finding the best foreground color ---
		vec3 foreground_color = center_color;
		bool foreground_found = false;
		float max_foreground_contribution = 0.0;

		// --- Track minimum depth neighbor data (for DEPTH output and fallback color) ---
		vec3 min_depth_neighbor_color = center_color;
		float min_depth = center_depth;
		vec2 min_depth_data = center_depth_data;

		// --- Loop through neighboring pixels ---
		for (float x = -depthline_thickness; x <= depthline_thickness; x += 1.0) {
			for (float y = -depthline_thickness; y <= depthline_thickness; y += 1.0) {
				float pixel_dist_sq = x*x + y*y;
				// Skip center pixel and pixels outside the circular radius
				if ((x == 0.0 && y == 0.0) || (depthline_thickness * depthline_thickness < pixel_dist_sq)) {
					continue;
				}

				vec2 offset = vec2(x, y) * pixel_size;
				vec2 neighbor_uv = center_uv + offset;
				vec2 clamped_neighbor_uv = clamp(neighbor_uv, vec2(0.0), vec2(1.0));

				// Get neighbor depth data
				vec2 neighbor_depth_data = getDepth(clamped_neighbor_uv, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
				float neighbor_depth = neighbor_depth_data.x;

				// Skip processing this neighbor if it's skybox/infinite depth
				if (neighbor_depth >= 9999.0) continue;

				// Get halfway depth data for derivative calculation
				vec2 halfway_depth_data = getDepth(clamp(center_uv + 0.5 * offset, vec2(0.0), vec2(1.0)), DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
				float halfway_depth = halfway_depth_data.x;

				// Calculate Depth Contribution
				float depth_second_derivative = clamp(abs((center_depth - halfway_depth) - (halfway_depth - neighbor_depth)), 0.0, 1.0);
				float depth_first_derivative = clamp(abs(center_depth - neighbor_depth), 0.0, 1.0);
				float depth_contribution = (depth_first_derivative / max(pixel_dist_sq, 1.0)) * depth_second_derivative * depth_second_derivative * depth_sensitivity;
				depth_contribution = clamp(depth_contribution, 0.0, 1.0);

				// Calculate Normal Contribution
				vec3 neighbor_normal = texture(NORMAL_TEXTURE, clamped_neighbor_uv).rgb * 2.0 - 1.0;
				float normal_difference = 1.0 - abs(dot(neighbor_normal, center_normal));
				float normal_contribution = normal_sensitivity * normal_difference / max(min(halfway_depth, center_depth), 0.1); // Avoid division by zero

				// Accumulate edge score for this neighbor
				float current_total_contribution = depth_contribution + normal_contribution;
				edge_score += current_total_contribution;

				// Check if this neighbor is a candidate for foreground color
				bool is_closer_foreground = neighbor_depth < (center_depth - foreground_depth_bias);
				if (is_closer_foreground && current_total_contribution > max_foreground_contribution) {
					max_foreground_contribution = current_total_contribution;
					foreground_color = texture(SCREEN_TEXTURE, clamped_neighbor_uv).rgb;
					foreground_found = true;
				}

				// Track minimum depth neighbor data (Fallback Color & DEPTH Write)
				if (neighbor_depth < min_depth) {
					min_depth = neighbor_depth;
					min_depth_neighbor_color = texture(SCREEN_TEXTURE, clamped_neighbor_uv).rgb;
					min_depth_data = neighbor_depth_data; // Store depth data (vec2)
				}
			} // End y loop
		} // End x loop
		// --- End Neighbor Loop ---

		// Apply thresholding to get base outline alpha based on edge detection
		float valid_threshold_min = min(outline_threshold_min, outline_threshold_max);
		float valid_threshold_max = max(outline_threshold_min, outline_threshold_max);
		// Ensure max is slightly larger than min if they are equal, for valid smoothstep range
		if (valid_threshold_min >= valid_threshold_max) {
			valid_threshold_max = valid_threshold_min + 0.001;
		}
		// This determines WHERE the outline appears based on edges
		float edge_detection_alpha = smoothstep(valid_threshold_min, valid_threshold_max, edge_score);

		// Determine the base color for the outline
		vec3 base_outline_color;
		if (foreground_found) {
			base_outline_color = foreground_color;
		} else {
			base_outline_color = min_depth_neighbor_color; // Fallback to closest neighbor color
		}

		// Tint the chosen base color
		vec3 final_outline_color = base_outline_color * depthline_color;

		// --- Set Final Shader Outputs for Spatial ---
		ALBEDO = final_outline_color;

		// Calculate the final alpha:
		// Modulate the edge detection alpha by the desired opacity of the tint color itself.
		// If shadow_color_opacity is 1.0, final alpha is purely edge detection.
		// If shadow_color_opacity is 0.0, the outline is fully transparent regardless of edges.
		ALPHA = edge_detection_alpha * colorblend_factor;

		// Write depth: blend between original raw depth and minimum neighbor raw depth
		// Use the RAW depth value (.y) from the vec2 data for DEPTH output
		DEPTH = mix(center_depth_data.y, min_depth_data.y, edge_detection_alpha); // Usually depth blending should still follow edge detection alpha

	} // --- End else block (valid depth) ---
} // --- End fragment() ---