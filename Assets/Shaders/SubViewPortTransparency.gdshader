shader_type canvas_item;

uniform vec4 transp_color1 : source_color = vec4(1.0, 0.0, 0.0, 1.0); // Red
//uniform vec4 transp_color2 : source_color = vec4(0.0, 1.0, 0.0, 1.0); // Green
//uniform vec4 transp_color3 : source_color = vec4(0.0, 0.0, 1.0, 1.0); // Blue
//uniform vec4 transp_color4 : source_color = vec4(0.0, 0.0, 1.0, 1.0); // Blue
//uniform vec4 transp_color5 : source_color = vec4(0.0, 0.0, 1.0, 1.0); // Blue

uniform float tolerance : hint_range(0.0, 1.0) = 0.009;

void fragment() {
    vec4 current_color = texture(TEXTURE, UV);
    bool make_transparent = false;

    // Calculate differences for each target color
    float diff1 = dot(current_color.rgb - transp_color1.rgb, current_color.rgb - transp_color1.rgb);
    //float diff2 = dot(current_color.rgb - transp_color2.rgb, current_color.rgb - transp_color2.rgb);
    //float diff3 = dot(current_color.rgb - transp_color3.rgb, current_color.rgb - transp_color3.rgb);
	//float diff4 = dot(current_color.rgb - transp_color4.rgb, current_color.rgb - transp_color4.rgb);
	//float diff5 = dot(current_color.rgb - transp_color5.rgb, current_color.rgb - transp_color5.rgb);

    // Check if any of the differences are within the tolerance
    if (diff1 < tolerance * tolerance) {
        make_transparent = true;
    }
    //if (diff2 < tolerance * tolerance) {
    //    make_transparent = true;
    //}
    //if (diff3 < tolerance * tolerance) {
    //    make_transparent = true;
    //}
	//if (diff4 < tolerance * tolerance) {
    //    make_transparent = true;
    //}
	//if (diff5 < tolerance * tolerance) {
    //   make_transparent = true;
    //}

    // Apply transparency based on the checks
    if (make_transparent) {
        COLOR = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
        COLOR = current_color;
    }
}
