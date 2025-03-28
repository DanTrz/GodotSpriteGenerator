shader_type spatial;
render_mode unshaded, fog_disabled;

uniform float target_resolution : hint_range(32.0, 2048.0, 32.0) = 512.0;
uniform float outline_thickness: hint_range(0.1, 5.0, 0.1) = 1.0;
uniform float normal_threshold: hint_range(0.0, 2.0, 0.05) = 0.3; // Adjust this for sensitivity
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

uniform sampler2D screen_texture: hint_screen_texture, filter_nearest;
uniform sampler2D depth_texture: hint_depth_texture, filter_nearest;
uniform sampler2D normal_texture: hint_normal_roughness_texture, filter_nearest;

void vertex() {
POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

vec3 get_normal(vec2 uv) {
// Decode the normal from the normal/roughness texture
vec4 n_r = texture(normal_texture, uv);
vec3 normal = normalize(n_r.rgb * 2.0 - 1.0);
return normal;
}

void fragment() {
vec2 uv = floor(SCREEN_UV * target_resolution) / target_resolution;
vec2 pixel_size = vec2(1.0 / target_resolution, 1.0 / target_resolution);

vec3 current_normal = get_normal(uv);
bool is_outline = false;

for (float oy = -outline_thickness; oy <= outline_thickness; oy++) {
    for (float ox = -outline_thickness; ox <= outline_thickness; ox++) {
        if (abs(ox) < 0.5 && abs(oy) < 0.5) continue;

        vec2 neighbor_uv = uv + vec2(ox * pixel_size.x, oy * pixel_size.y);
        neighbor_uv = clamp(neighbor_uv, vec2(0.0), vec2(1.0));

        // Sample neighbor's normal
        vec3 neighbor_normal = get_normal(neighbor_uv);

        // Check the dot product of the normals. A large difference (dot product closer to -1) indicates an edge.
        if (dot(current_normal, neighbor_normal) < (1.0 - normal_threshold)) {
            is_outline = true;
            break; // Found an outline, no need to check further neighbors for this pixel
        }
    }
    if (is_outline) break;
}

vec3 color = texture(screen_texture, uv).rgb;
ALBEDO = mix(color, outline_color.rgb, float(is_outline));
}
