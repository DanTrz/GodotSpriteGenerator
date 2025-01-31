shader_type spatial;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float pixel_size = 128.0;

void fragment()
{
    vec2 uv = SCREEN_UV;
    vec2 pixel_uv = floor(uv * pixel_size) / pixel_size;
    vec4 color = texture(screen_texture, pixel_uv);
    ALBEDO = color.rgb;
}
