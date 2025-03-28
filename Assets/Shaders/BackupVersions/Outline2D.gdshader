shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float outline_thickness : hint_range(0.0, 5.0, 0.1) = 1.0;
uniform vec4 inline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float inline_thickness : hint_range(0.0, 5.0, 0.1) = 0.0; // Default to 0 for no inline initially

void fragment() {
vec2 uv = UV;
vec4 original_color = texture(TEXTURE, uv);
COLOR = original_color; // Start with the original color

// Outline effect
if (original_color.a == 0.0 && outline_thickness > 0.0) {
	bool has_opaque_neighbor = false;
	for (int y = -int(outline_thickness); y <= int(outline_thickness); y++) {
		for (int x = -int(outline_thickness); x <= int(outline_thickness); x++) {
			if (x == 0 && y == 0) continue;

			vec2 neighbor_uv = uv + vec2(float(x), float(y)) * TEXTURE_PIXEL_SIZE;
			vec4 neighbor_color = texture(TEXTURE, clamp(neighbor_uv, vec2(0.0), vec2(1.0)));

			if (neighbor_color.a > 0.0) {
				has_opaque_neighbor = true;
				break;
			}
		}
		if (has_opaque_neighbor) break;
	}
	if (has_opaque_neighbor) {
		COLOR = outline_color;
	}
}
// Inline effect (only if original pixel is opaque)
else if (original_color.a > 0.0 && inline_thickness > 0.0) {
	bool has_transparent_neighbor = false;
	for (int y = -int(inline_thickness); y <= int(inline_thickness); y++) {
		for (int x = -int(inline_thickness); x <= int(inline_thickness); x++) {
			if (x == 0 && y == 0) continue;

			vec2 neighbor_uv = uv + vec2(float(x), float(y)) * TEXTURE_PIXEL_SIZE;
			vec4 neighbor_color = texture(TEXTURE, clamp(neighbor_uv, vec2(0.0), vec2(1.0)));

			if (neighbor_color.a == 0.0) {
				has_transparent_neighbor = true;
				break;
			}
		}
		if (has_transparent_neighbor) break;
	}
	if (has_transparent_neighbor) {
		// Simple logic: if any transparent neighbor, apply inline color
		COLOR = inline_color;
	}
}
}
