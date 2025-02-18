shader_type spatial;
render_mode cull_front, unshaded; // Render only front-facing polygons to create an outline effect

// Uniforms for controlling outline properties
uniform vec4 outline_color : source_color; // Color of the outline
uniform float outline_width : hint_range(0.0, 5.0) = 1.0; // Thickness of the outline
uniform float fade_start : hint_range(0.0, 100.0) = 10.0; // Distance at which fading starts
uniform float fade_end : hint_range(0.0, 100.0) = 30.0; // Distance at which outline becomes invisible

void vertex() {
	// Convert the vertex position to clip space
	vec4 clip_position = PROJECTION_MATRIX * (MODELVIEW_MATRIX * vec4(VERTEX, 1.0));

	// Transform the normal into clip space
	vec3 clip_normal = mat3(PROJECTION_MATRIX * MODELVIEW_MATRIX) * NORMAL;

	// Calculate offset for outline expansion
	vec2 offset = normalize(clip_normal.xy) * outline_width * clip_position.w / VIEWPORT_SIZE;

	// Apply the offset to create the outline effect
	clip_position.xy += offset * 2.0;

	// Set final position
	POSITION = clip_position;
}

void fragment() {
	// Get the distance from the camera
	float dist = length(VERTEX);

	// Compute fade factor based on distance
	float fade = clamp(1.0 - (dist - fade_start) / (fade_end - fade_start), 0.0, 1.0);

	// Apply fade effect
	ALBEDO = outline_color.rgb;
	ALPHA = outline_color.a * fade; // Multiply alpha by fade factor
}
