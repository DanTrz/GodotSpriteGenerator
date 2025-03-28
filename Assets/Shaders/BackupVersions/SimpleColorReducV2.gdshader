shader_type canvas_item;

uniform int levels : hint_range(1, 256) = 256; // Number of color levels

void fragment() {
    vec3 color = texture(TEXTURE, UV).rgb;

    // Reduce colors by snapping them to the closest level
    //color = floor(color * float(max_rgb_color_levels)) / float(max_rgb_color_levels);

	// Apply color quantization
    color.rgb = floor(color.rgb * float(levels)) / float(levels);

    COLOR = vec4(color, texture(TEXTURE, UV).a);
}
