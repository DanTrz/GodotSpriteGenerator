#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba16f, set = 0, binding = 0) uniform image2D color_image;
layout(rgba16f, set = 0, binding = 1) uniform image2D depth_image;

layout(push_constant, std430) uniform Params {
	vec2 raster_size;
	float outline_thickness;
	vec4 outline_color;
	float depth_threshold;
} params;

float get_depth(ivec2 uv_coord) {
	ivec2 size = ivec2(params.raster_size);
	if (uv_coord.x < 0 || uv_coord.x >= size.x || uv_coord.y < 0 || uv_coord.y >= size.y) {
		return 1.0;
	}
	return imageLoad(depth_image, uv_coord).r;
}

void main() {
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);
	ivec2 size = ivec2(params.raster_size);

	if (uv.x >= size.x || uv.y >= size.y) {
		return;
	}

	vec4 color = imageLoad(color_image, uv);
	float depth = get_depth(uv);

	float is_edge = 0.0;
	float pixel_size_x = 1.0 / params.raster_size.x;
	float pixel_size_y = 1.0 / params.raster_size.y;

	// Simple depth difference check (horizontal and vertical)
	float depth_left = get_depth(uv - ivec2(1, 0));
	float depth_right = get_depth(uv + ivec2(1, 0));
	float depth_up = get_depth(uv - ivec2(0, 1));
	float depth_down = get_depth(uv + ivec2(0, 1));

	float depth_diff_x = abs(depth - depth_left);
	float depth_diff_y = abs(depth - depth_up);

	if (depth_diff_x > params.depth_threshold || depth_diff_y > params.depth_threshold) {
		is_edge = 1.0;
	}

	// Apply outline color if an edge is detected
	if (is_edge > 0.0) {
		color = params.outline_color;
	}

	imageStore(color_image, uv, color);
}