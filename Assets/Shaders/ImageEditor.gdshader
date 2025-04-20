shader_type canvas_item;
render_mode unshaded;

// Brightness, Contrast, and Saturation Controls (Matrix Method)
uniform float brightness : hint_range(-1.0, 1.0, 0.01) = 0.0;
uniform float contrast : hint_range(-5.0, 5.0, 0.01) = 1.0;
uniform float saturation : hint_range(-5.0, 5.0, 0.01) = 1.0;

// Color Reduction Controls (HSL Method)
uniform bool enable_color_reduction = true;
uniform bool enable_external_palette = false;
uniform int num_colors : hint_range(2, 512) = 16;
uniform int new_num_colors : hint_range(2, 512) = 16;
uniform vec4 palette[256];

// Outline and Inline Controls
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float outline_thickness : hint_range(0.0, 20.0, 0.1) = 0.0;
uniform vec4 inline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float inline_thickness : hint_range(0.0, 20.0, 0.1) = 0.0;

// Helper function for calculating color distance
float color_distance(vec3 c1, vec3 c2) {
    return distance(c1, c2);
}

// Matrix functions for brightness, contrast, and saturation
mat4 brightness_matrix(float b) {
    return mat4(
        vec4(1, 0, 0, 0),
        vec4(0, 1, 0, 0),
        vec4(0, 0, 1, 0),
        vec4(b, b, b, 1)
    );
}

mat4 contrast_matrix(float c) {
    float t = (1.0 - c) / 2.0;
    return mat4(
        vec4(c, 0, 0, 0),
        vec4(0, c, 0, 0),
        vec4(0, 0, c, 0),
        vec4(t, t, t, 1)
    );
}

mat4 saturation_matrix(float s) {
    vec3 luminance = vec3(0.3086, 0.6094, 0.0820);
    float sr = (1.0 - s) * luminance.r;
    float sg = (1.0 - s) * luminance.g;
    float sb = (1.0 - s) * luminance.b;
    return mat4(
        vec4(sr + s, sr, sr, 0),
        vec4(sg, sg + s, sg, 0),
        vec4(sb, sb, sb + s, 0),
        vec4(0, 0, 0, 1)
    );
}

void fragment() {
    vec4 image_color = texture(TEXTURE, UV);
    vec4 color = image_color;

    // Apply color reduction FIRST (HSL Method)
    if (enable_color_reduction || enable_external_palette) {
        vec3 best_match = palette[0].rgb;
        float min_distance = color_distance(color.rgb, best_match);

        for (int i = 1; i < num_colors; i++) {
            float dist = color_distance(color.rgb, palette[i].rgb);
            if (dist < min_distance) {
                min_distance = dist;
                best_match = palette[i].rgb;
            }
        }
        color.rgb = best_match;
    }

    // Apply brightness, contrast, and saturation (Matrix Method)
    mat4 matrix = brightness_matrix(brightness) * contrast_matrix(contrast) * saturation_matrix(saturation);
    color = matrix * color;

    // Store processed color
    vec4 final_color = color;
    vec2 uv = UV;
    bool has_opaque_neighbor = false;
    bool has_transparent_neighbor = false;

    // Check for outline separately
    if (outline_thickness > 0.0 && image_color.a == 0.0) {
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
            final_color = outline_color;
        }
    }

    // Check for inline separately
    if (inline_thickness > 0.0 && image_color.a > 0.0) {
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
            final_color = inline_color;
        }
    }

    COLOR = final_color;
}
