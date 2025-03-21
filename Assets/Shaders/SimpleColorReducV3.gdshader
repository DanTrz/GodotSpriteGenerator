shader_type canvas_item;

uniform int levels : hint_range(1, 32) = 20; //Max 256 (Full RGB)
uniform float dither_strength : hint_range(0.0, 1.0) = 0.0; // 0.2 to 0.4 are the recommended - Adjustable dither strength

void fragment() {
    vec3 color = texture(TEXTURE, UV).rgb;

    // Generate a single random value
    float dither = fract(sin(dot(UV, vec2(12.9898, 78.233))) * 43758.5453);

    // Apply the dither with adjustable strength
    color += (dither - 0.5) * dither_strength / float(levels);

    // Apply color quantization
    color.rgb = floor(color.rgb * float(levels)) / float(levels);

    COLOR = vec4(color, texture(TEXTURE, UV).a);
}