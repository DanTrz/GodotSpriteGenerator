shader_type spatial;
render_mode unshaded, cull_disabled;

uniform float outline_size : hint_range(0.0, 0.1, 0.001) = 0.02;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void vertex() {
	VERTEX += NORMAL * outline_size;
	VERTEX *= (1.0 + outline_size * 0.5); // Slight scaling
}

void fragment() {
	ALBEDO = outline_color.rgb;
	ALPHA = outline_color.a;
}
