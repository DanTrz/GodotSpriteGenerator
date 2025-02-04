shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float pixel_size = 128.0;

void fragment() {
    vec2 uv_pixel = floor(UV * pixel_size) / pixel_size;
    vec4 col = texture(screen_texture, uv_pixel);
    COLOR = col;
}
