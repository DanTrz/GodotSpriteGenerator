shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float target_resolution : hint_range(1.0, 512.0, 1.0) = 512.0;
uniform float edge_smoothness : hint_range(0.0, 1.0) = 0.1;

// Color removal parameters
uniform vec4 transp_color1 : source_color = vec4(1.0, 0.0, 0.0, 1.0); // Red
//uniform vec4 transp_color2 : source_color = vec4(0.0, 1.0, 0.0, 1.0); // Green
//uniform vec4 transp_color3 : source_color = vec4(0.0, 0.0, 1.0, 1.0); // Blue
//uniform vec4 transp_color4 : source_color = vec4(0.0, 0.0, 1.0, 1.0); // Blue
//uniform vec4 transp_color5 : source_color = vec4(0.0, 0.0, 1.0, 1.0); // Blue
uniform float tolerance : hint_range(0.0, 1.0) = 0.009;

// Check if a color should be transparent
bool should_be_transparent(vec4 color) {
    // Calculate differences for each target color
    float diff1 = dot(color.rgb - transp_color1.rgb, color.rgb - transp_color1.rgb);
    //float diff2 = dot(color.rgb - transp_color2.rgb, color.rgb - transp_color2.rgb);
    //float diff3 = dot(color.rgb - transp_color3.rgb, color.rgb - transp_color3.rgb);
    //float diff4 = dot(color.rgb - transp_color4.rgb, color.rgb - transp_color4.rgb);
    //float diff5 = dot(color.rgb - transp_color5.rgb, color.rgb - transp_color5.rgb);
    
    // Check if any of the differences are within the tolerance
    float tolerance_squared = tolerance * tolerance;
    
    if (diff1 < tolerance_squared) {
        return true;
    }
    //if (diff2 < tolerance_squared) {
    //    return true;
    //}
    //if (diff3 < tolerance_squared) {
    //    return true;
    //}
    //if (diff4 < tolerance_squared) {
    //    return true;
    //}
    //if (diff5 < tolerance_squared) {
    //    return true;
    //}
    
    return false;
}

// Improved point smoothing function with better anti-aliasing
vec4 texturePointSmoothEnhanced(sampler2D smp, vec2 uv, vec2 pixel_size, float smoothness)
{
    // Calculate derivatives for proper filtering
    vec2 ddx = dFdx(uv);
    vec2 ddy = dFdy(uv);
    
    // Calculate screen-space texel size
    vec2 lxy = sqrt(ddx * ddx + ddy * ddy);
    
    // Convert UV to pixel space
    vec2 uv_pixels = uv / pixel_size;
    
    // Find nearest pixel center
    vec2 uv_pixels_center = floor(uv_pixels) + 0.5;
    
    // Calculate distance from pixel center in pixel space
    vec2 dist_center = uv_pixels - uv_pixels_center;
    
    // Calculate anti-aliasing factor, scaled by edge smoothness
    vec2 aa_factor = smoothness * lxy / pixel_size;
    
    // Apply anti-aliasing smoothstep
    vec2 pixel_blend = smoothstep(
        vec2(-0.5 - aa_factor), 
        vec2(-0.5 + aa_factor), 
        dist_center
    ) * (1.0 - smoothstep(
        vec2(0.5 - aa_factor), 
        vec2(0.5 + aa_factor), 
        dist_center
    ));
    
    // Find the four nearest texels
    vec2 uv00 = (uv_pixels_center - vec2(0.5, 0.5)) * pixel_size;
    vec2 uv10 = (uv_pixels_center + vec2(0.5, -0.5)) * pixel_size;
    vec2 uv01 = (uv_pixels_center + vec2(-0.5, 0.5)) * pixel_size;
    vec2 uv11 = (uv_pixels_center + vec2(0.5, 0.5)) * pixel_size;
    
    // Sample the four texels with proper gradients
    vec4 col00 = textureGrad(smp, uv00, ddx, ddy);
    vec4 col10 = textureGrad(smp, uv10, ddx, ddy);
    vec4 col01 = textureGrad(smp, uv01, ddx, ddy);
    vec4 col11 = textureGrad(smp, uv11, ddx, ddy);
    
    // Apply transparency to each sampled color if needed
    if (should_be_transparent(col00)) col00.a = 0.0;
    if (should_be_transparent(col10)) col10.a = 0.0;
    if (should_be_transparent(col01)) col01.a = 0.0;
    if (should_be_transparent(col11)) col11.a = 0.0;
    
    // Bilinear blend between the four texels
    vec4 col = mix(
        mix(col00, col10, pixel_blend.x),
        mix(col01, col11, pixel_blend.x),
        pixel_blend.y
    );
    
    return col;
}

void fragment() {
    // Get screen dimensions and pixel aspect ratio correction
    vec2 screen_size = 1.0/SCREEN_PIXEL_SIZE;
    
    // Calculate pixel size, ensuring it's an integer for consistent sizing
    float pixel_size = max(floor(min(screen_size.x, screen_size.y) / target_resolution), 1.0);
    
    // Calculate grid-aligned UVs with high precision
    vec2 grid_uv = floor(SCREEN_UV * screen_size / pixel_size + 0.5) * pixel_size / screen_size;
    
    // First check if the original color should be transparent
    vec4 original_color = texture(screen_texture, SCREEN_UV);
    
    // Apply enhanced smoothing with built-in transparency handling
    vec4 processed_color = texturePointSmoothEnhanced(screen_texture, grid_uv, SCREEN_PIXEL_SIZE, edge_smoothness);
    
    // If the original color should be transparent, set alpha to 0
    if (should_be_transparent(original_color)) {
        COLOR = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
        COLOR = processed_color;
    }
}
