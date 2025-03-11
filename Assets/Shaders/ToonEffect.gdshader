shader_type spatial;

render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_toon,specular_toon;
uniform vec4 albedo : source_color;
uniform sampler2D color_gradient;
uniform sampler2D fresnel_gradient;


float fresnel(float amount, vec3 normal, vec3 view)
{
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0 )), amount);
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float basic_fresnel = fresnel(4.0, NORMAL, VIEW);
	ALBEDO = albedo.rgb * (basic_fresnel * texture(fresnel_gradient, vec2(basic_fresnel, 0.0)).rgb * 1.0);

}

void light() {
	float dotproduct = clamp(dot(NORMAL, LIGHT), -0.9, 0.9);
	float sample = clamp((dotproduct + 1.0) * ATTENUATION / 2.1, 0.05, 0.95);
	vec4 shaded = texture(color_gradient, vec2(sample, 0.0));
	DIFFUSE_LIGHT += clamp((shaded.rgb * LIGHT_COLOR), 0.0, 1.0);
	//DIFFUSE_LIGHT += (shaded.rgb * LIGHT_COLOR);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
