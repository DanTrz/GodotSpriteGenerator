shader_type spatial;
// Added cull_front to render the correct face when QuadMesh has Flip Faces enabled.
render_mode unshaded, depth_draw_opaque, depth_prepass_alpha, cull_front;

// Inspired by https://godotshaders.com/shader/3d-pixel-art-outline-highlight-post-processing-shader/

// Original Inputs
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D NORMAL_TEXTURE : hint_normal_roughness_texture, filter_nearest;

// Outline Appearance
uniform vec3 shadow_color : source_color = vec3(0.0);
uniform float shadow_thickness : hint_range(1.0, 10.0) = 2.0;

// --- New Sensitivity Controls ---
// Controls how much weight is given to depth differences. Higher values detect smaller depth changes.
uniform float depth_sensitivity : hint_range(1.0, 2000.0) = 500.0;
// Controls how much weight is given to normal differences. Higher values react more to surface angle changes.
uniform float normal_sensitivity : hint_range(0.0, 10.0) = 1.0;
// The minimum accumulated difference required to start drawing the outline.
uniform float outline_threshold_min : hint_range(0.0, 1.0) = 0.2;
// The accumulated difference required to draw the outline at full opacity.
uniform float outline_threshold_max : hint_range(0.0, 1.0) = 0.3;
// --- End New Sensitivity Controls ---


// Function to get linear view-space depth and raw depth from depth texture
vec2 getDepth(vec2 screen_uv, sampler2D depth_texture, mat4 inv_projection_matrix){
	float raw_depth = texture(depth_texture, screen_uv)[0];
	// Convert depth from non-linear clip space to linear view space
	vec3 normalized_device_coordinates = vec3(screen_uv * 2.0 - 1.0, raw_depth);
	vec4 view_space = inv_projection_matrix * vec4(normalized_device_coordinates, 1.0);
	view_space.xyz /= view_space.w;
	// Return linear depth (-Z in view space) and raw depth
	return vec2(-view_space.z, raw_depth);
}


void fragment() {
	// Calculate pixel size for UV offsets
	vec2 pixel_size = vec2(1.0 / VIEWPORT_SIZE.xy);

	// Accumulator for edge detection score
	float edge_score = 0.0;

	// Get depth data for the current fragment
	vec2 center_depth_data = getDepth(SCREEN_UV, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
	float center_depth = center_depth_data.x; // Linear view-space depth

	// Get color and normal for the current fragment
	vec3 center_color = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
	vec3 center_normal = texture(NORMAL_TEXTURE, SCREEN_UV).rgb * 2.0 - 1.0; // Unpack normal

	// Variables to store data from the closest neighbor (used for outline color/depth)
	vec2 min_depth_data = center_depth_data;
	float min_depth = 9999999.9;
	vec3 neighbor_color = vec3(0.0); // Color of the neighbor potentially causing the outline


	// Loop through neighboring pixels in a circular pattern
	for (float x = -shadow_thickness; x <= shadow_thickness; x += 1.0) {
		for (float y = -shadow_thickness; y <= shadow_thickness; y += 1.0) {
			// Skip the center pixel and pixels outside the defined radius
			float pixel_dist_sq = x*x + y*y;
			if ((x == 0.0 && y == 0.0) || (shadow_thickness * shadow_thickness < pixel_dist_sq)) {
				continue;
			}

			// Calculate UV offset for the neighbor
			vec2 offset = vec2(x, y) * pixel_size;
			vec2 neighbor_uv = SCREEN_UV + offset;

			// Get depth data for two points along the offset direction (neighbor and halfway)
			vec2 neighbor_depth_data = getDepth(neighbor_uv, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
			vec2 halfway_depth_data = getDepth(SCREEN_UV + 0.5 * offset, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);

			float neighbor_depth = neighbor_depth_data.x;
			float halfway_depth = halfway_depth_data.x;

			// --- Depth Contribution ---
			// Calculate second derivative of depth to detect sharp changes
			float depth_second_derivative = clamp(abs((center_depth - halfway_depth) - (halfway_depth - neighbor_depth)), 0.0, 1.0);
			// Calculate first derivative (direct depth difference)
			float depth_first_derivative = clamp(abs(center_depth - neighbor_depth), 0.0, 1.0);
			// Combine derivatives, scale by distance and sensitivity. Using max(pixel_dist_sq, 1.0) to avoid division by zero potential if thickness < 1.
			float depth_contribution = (depth_first_derivative / max(pixel_dist_sq, 1.0)) * depth_second_derivative * depth_second_derivative * depth_sensitivity;
			depth_contribution = clamp(depth_contribution, 0.0, 1.0); // Clamp contribution per neighbor
			edge_score += depth_contribution;
			// --- End Depth Contribution ---


			// --- Normal Contribution ---
			vec3 neighbor_normal = texture(NORMAL_TEXTURE, neighbor_uv).rgb * 2.0 - 1.0;
			// Calculate difference based on dot product (0 for same normal, 1 for perpendicular)
			float normal_difference = 1.0 - abs(dot(neighbor_normal, center_normal));
			// Scale by sensitivity. The division by depth makes it less sensitive for far objects.
			// Using max(..., 0.1) to avoid extreme sensitivity increase at very close depths.
			float normal_contribution = normal_sensitivity * normal_difference / max(min(halfway_depth, center_depth), 0.1);
			edge_score += normal_contribution;
			// --- End Normal Contribution ---


			// Keep track of the nearest neighbor found so far
			if (neighbor_depth < min_depth) {
				min_depth = neighbor_depth;
				min_depth_data = neighbor_depth_data;
				// Get color from the neighbor, potentially modulating by direction (optional)
				neighbor_color = texture(SCREEN_TEXTURE, neighbor_uv).rgb;
				// Optional: Modulate color by direction relative to view (e.g., top lit)
				// neighbor_color *= clamp(0.5 + 0.5 * dot(normalize(vec2(x, y)), vec2(0.0, 1.0)), 0.0, 1.0);
			}
		}
	}

	// Apply thresholding using smoothstep for a soft transition
	// Ensure min is not greater than max to avoid issues
	float valid_threshold_min = min(outline_threshold_min, outline_threshold_max);
	float valid_threshold_max = max(outline_threshold_min, outline_threshold_max);
	// Ensure there's a small gap if min and max are equal
	if (valid_threshold_min == valid_threshold_max) {
		valid_threshold_max += 0.001;
	}

	float outline_alpha = smoothstep(valid_threshold_min, valid_threshold_max, edge_score);

	// Final color is the neighbor's color tinted by the shadow color
	// Using neighbor_color helps the outline blend with the object behind the edge
	vec3 final_color = neighbor_color * shadow_color;
	ALBEDO = final_color;

	// Write depth: blend between original depth and neighbor depth based on outline alpha
	// This helps the outline appear "on top" of the closer surface at the edge
	DEPTH = mix(center_depth_data.y, min_depth_data.y, outline_alpha);

	// Set final alpha based on the calculated outline intensity
	ALPHA = outline_alpha;
}
