shader_type canvas_item;

uniform bool enable_saturation = true;
uniform float saturation : hint_range(0.0, 2.0) = 1.0;
uniform bool enable_brightness = true;
uniform float brightness : hint_range(0.0, 2.0) = 1.0;

// Convert RGB to HSL
vec3 rgb_to_hsl(vec3 color) {
    float maxC = max(color.r, max(color.g, color.b));
    float minC = min(color.r, min(color.g, color.b));
    float delta = maxC - minC;

    float h = 0.0;
    float s = 0.0;
    float l = (maxC + minC) * 0.5;

    if (delta > 0.00001) {
        s = (l < 0.5) ? (delta / (maxC + minC)) : (delta / (2.0 - maxC - minC));

        if (maxC == color.r) {
            h = mod((color.g - color.b) / delta, 6.0);
        } else if (maxC == color.g) {
            h = (color.b - color.r) / delta + 2.0;
        } else {
            h = (color.r - color.g) / delta + 4.0;
        }

        h /= 6.0;
    }

    return vec3(h, s, l);
}

// Convert HSL to RGB
vec3 hsl_to_rgb(vec3 hsl) {
    float h = hsl.x;
    float s = hsl.y;
    float l = hsl.z;

    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    float x = c * (1.0 - abs(mod(h * 6.0, 2.0) - 1.0));
    float m = l - c * 0.5;

    vec3 rgb;

    if (h < 1.0 / 6.0) {
        rgb = vec3(c, x, 0.0);
    } else if (h < 2.0 / 6.0) {
        rgb = vec3(x, c, 0.0);
    } else if (h < 3.0 / 6.0) {
        rgb = vec3(0.0, c, x);
    } else if (h < 4.0 / 6.0) {
        rgb = vec3(0.0, x, c);
    } else if (h < 5.0 / 6.0) {
        rgb = vec3(x, 0.0, c);
    } else {
        rgb = vec3(c, 0.0, x);
    }

    return rgb + vec3(m);
}

void fragment() {
    vec4 color = texture(TEXTURE, UV);

    if (enable_saturation || enable_brightness) {
        vec3 hsl = rgb_to_hsl(color.rgb);

        if (enable_saturation) {
            hsl.y = clamp(hsl.y * saturation, 0.0, 1.0);
        }

        if (enable_brightness) {
            hsl.z = clamp(hsl.z * brightness, 0.0, 1.0);
        }

        color.rgb = hsl_to_rgb(hsl);
    }

    COLOR = color;
}
