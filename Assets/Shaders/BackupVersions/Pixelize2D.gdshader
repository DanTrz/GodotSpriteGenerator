shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, repeat_disable, filter_nearest;
uniform float pixel_size_pixels : hint_range(1.0, 256.0, 1.0) = 16.0; // Changed to pixel count
uniform vec2 viewport_size; // To get the viewport dimensions

void fragment() {
	vec2 uv = FRAGCOORD.xy / viewport_size; // Normalized UV coordinates (0.0 to 1.0)
	vec2 pixel_size_uv = viewport_size / pixel_size_pixels; // Size of a single pixel in UV space

	vec2 pixel_coord = floor(uv * pixel_size_pixels); // Pixel coordinate within the grid
	vec2 pixel_uv = (pixel_coord + 0.5) / pixel_size_pixels; // Center of the pixel in UV space

	COLOR = texture(SCREEN_TEXTURE, pixel_uv);
}