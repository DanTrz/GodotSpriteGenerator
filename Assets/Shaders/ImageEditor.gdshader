shader_type canvas_item;

// Saturation and Brightness Controls
uniform bool enable_saturation = false;
uniform float saturation : hint_range(0.0, 2.0) = 1.0;
uniform bool enable_brightness = false;
uniform float brightness : hint_range(0.0, 2.0) = 1.0;

// Color Reduction Controls
uniform bool enable_color_reduction = true;
uniform int num_colors : hint_range(2, 256) = 16;
uniform bool use_external_palette = true;
uniform vec4 external_palette[256];

// Helper function for calculating color distance
float color_distance(vec3 c1, vec3 c2) {
    return distance(c1, c2);
}

// Corrected RGB to HSL conversion
vec3 rgb_to_hsl(vec3 c) {
    float c_max = max(max(c.r, c.g), c.b);
    float c_min = min(min(c.r, c.g), c.b);
    float delta = c_max - c_min;

    float h = 0.0;
    float s = 0.0;
    float l = (c_max + c_min) / 2.0;

    if (delta > 0.0) {
        s = l < 0.5 ? delta / (c_max + c_min) : delta / (2.0 - c_max - c_min);

        if (c.r == c_max)      h = (c.g - c.b) / delta;
        else if (c.g == c_max) h = (c.b - c.r) / delta + 2.0;
        else                   h = (c.r - c.g) / delta + 4.0;

        h /= 6.0;
        if (h < 0.0) h += 1.0;
    }

    return vec3(h, s, l);
}
// Corrected HSL to RGB conversion
vec3 hsl_to_rgb(vec3 c) {
    float h = c.x;
    float s = c.y;
    float l = c.z;

    float chroma = (1.0 - abs(2.0 * l - 1.0)) * s;
    float x = chroma * (1.0 - abs(mod(h * 6.0, 2.0) - 1.0));
    float m = l - chroma / 2.0;

    vec3 rgb;
    if (h < 1.0 / 6.0)      rgb = vec3(chroma, x, 0.0);
    else if (h < 2.0 / 6.0) rgb = vec3(x, chroma, 0.0);
    else if (h < 3.0 / 6.0) rgb = vec3(0.0, chroma, x);
    else if (h < 4.0 / 6.0) rgb = vec3(0.0, x, chroma);
    else if (h < 5.0 / 6.0) rgb = vec3(x, 0.0, chroma);
    else                    rgb = vec3(chroma, 0.0, x);

    return rgb + m;
}


void fragment() {
    vec4 image_color = texture(TEXTURE, UV);
    vec3 color = image_color.rgb;

    // --- Saturation and Brightness ---
    if (enable_saturation || enable_brightness) {
        vec3 hsl = rgb_to_hsl(color);

        if (enable_saturation) {
            hsl.y *= saturation;
            hsl.y = clamp(hsl.y, 0.0, 1.0);
        }

        if (enable_brightness) {
            hsl.z *= brightness;
            hsl.z = clamp(hsl.z, 0.0, 1.0);
        }

        color = hsl_to_rgb(hsl);
    }

    // --- Color Reduction ---
    if (enable_color_reduction) {
        vec3 best_match = external_palette[0].rgb;
        float min_distance = color_distance(color, best_match);

        for (int i = 1; i < num_colors; i++) {
            float dist = color_distance(color, external_palette[i].rgb);
            if (dist < min_distance) {
                min_distance = dist;
                best_match = external_palette[i].rgb;
            }
        }
        color = best_match;
    }

    COLOR = vec4(color, image_color.a);
}
