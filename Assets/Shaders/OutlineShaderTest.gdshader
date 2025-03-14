shader_type spatial;
render_mode unshaded, cull_disabled, fog_disabled;

uniform float outline_thickness : hint_range(0.001, 0.02, 0.001) = 0.005;
uniform vec3 outline_color : source_color = vec3(0.0, 0.0, 1);

uniform sampler2D depth_texture : hint_depth_texture, filter_nearest;
uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

void fragment() {
    vec2 uv = SCREEN_UV;
    float center_depth = texture(depth_texture, uv).r;

    // Sample neighboring depth values
    float left_depth = texture(depth_texture, uv - vec2(outline_thickness, 0)).r;
    float right_depth = texture(depth_texture, uv + vec2(outline_thickness, 0)).r;
    float up_depth = texture(depth_texture, uv + vec2(0, outline_thickness)).r;
    float down_depth = texture(depth_texture, uv - vec2(0, outline_thickness)).r;

    // Calculate depth difference
    float edge = abs(center_depth - left_depth) +
                 abs(center_depth - right_depth) +
                 abs(center_depth - up_depth) +
                 abs(center_depth - down_depth);

    // Threshold for outline visibility
    edge = step(0.002, edge); // Adjust threshold if needed

    // Apply outline color or keep original screen color
    vec3 screen_color = texture(screen_texture, uv).rgb;
    ALBEDO = mix(screen_color, outline_color, edge);
}




