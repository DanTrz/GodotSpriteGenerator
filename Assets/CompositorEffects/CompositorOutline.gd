@tool
extends CompositorEffect
class_name PostProcessOutline

@export var outline_thickness: float = 1.0
@export var outline_color: Color = Color(0, 0, 0, 1)
@export var depth_threshold: float = 0.02

var rd: RenderingDevice
var shader: RID
var pipeline: RID
var shader_effect: ShaderMaterial

func _ready() -> void:
	rd = RenderingServer.get_rendering_device()
	if not rd:
		push_error("RenderingDevice not available.")
		return

	# Load the shader
	var shader_file := load("res://outline.glsl")
	if not shader_file:
		push_error("Failed to load outline.glsl shader.")
		return
	var shader_spirv: RDShaderSPIRV = shader_file.get_spirv()
	shader = rd.shader_create_from_spirv(shader_spirv)
	if not shader.is_valid():
		push_error("Failed to create shader from SPIR-V.")
		return
	pipeline = rd.compute_pipeline_create(shader)
	if not pipeline.is_valid():
		push_error("Failed to create compute pipeline.")
		return

	# Create and add the ShaderEffect
	var material = ShaderMaterial.new()
	material.shader = shader
	
	shader_effect = ShaderMaterial.new()
	shader_effect.material = material

	# Assume environment is setup elsewhere if needed

	# Get the viewport size
	var viewport_size = get_viewport_size()
	if viewport_size == Vector2.ZERO:
		push_warning("Viewport size is zero.")
		return

	# Initialize the shader parameters
	update_shader_parameters(viewport_size)

func _process(delta: float) -> void:
	if shader_effect and shader_effect.material is ShaderMaterial:
		update_shader_parameters(get_viewport_size())


func update_shader_parameters(viewport_size: Vector2) -> void:
	if shader_effect.material is ShaderMaterial:
		var shader_mat = shader_effect.material as ShaderMaterial
		shader_mat.set_shader_param("raster_size", viewport_size)
		shader_mat.set_shader_param("outline_thickness", outline_thickness)
		shader_mat.set_shader_param("outline_color", outline_color)
		shader_mat.set_shader_param("depth_threshold", depth_threshold)

func _exit_tree() -> void:
	if shader.is_valid():
		rd.free_rid(shader)
	if pipeline.is_valid():
		rd.free_rid(pipeline)
