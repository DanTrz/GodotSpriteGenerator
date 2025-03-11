shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float pixel_size : hint_range(1.0, 512.0, 1.0) = 8.0;
uniform vec2 viewport_size = vec2(512.0, 512.0); // Or dynamically set via C#

void fragment() {
	//vec2 uv_pixel = floor(UV * viewport_size / pixel_size) * (pixel_size / viewport_size);
	 vec2 uv_pixel = floor(UV * pixel_size) / pixel_size;
	vec4 col = texture(screen_texture, uv_pixel);
	COLOR = vec4(col.rgb, col.a);
	
}
