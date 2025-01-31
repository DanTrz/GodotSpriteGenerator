using Godot;
using System;

public partial class Model3D : Node3D
{
	[Export] public Node3D MainMesh;
	[Export] public float RotationSpeed = 0.01f;
	[Export] public float VerticalAngleLimit = 180.0f;  // Limit vertical angle to prevent flipping.
	[Export] public float scrollSpeed = 1f;



	private float _horizontalAngle = 0.0f;
	private float _verticalAngle = 0.0f;
	private const float Rad2Deg = 180f / Mathf.Pi;  // Define Rad2Deg manually.



	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion && Input.IsActionPressed(Const.MOUSE_RIGHT_CLICK))
		{
			// Adjust the horizontal and vertical angles based on mouse movement
			_horizontalAngle -= mouseMotion.Relative.X * RotationSpeed;
			_verticalAngle = Mathf.Clamp(_verticalAngle - mouseMotion.Relative.Y * RotationSpeed, -VerticalAngleLimit, VerticalAngleLimit);

			// Apply the rotation to the actual MODEL keeping the camera in place
			MainMesh.RotationDegrees = new Vector3(-_verticalAngle * Rad2Deg, -_horizontalAngle * Rad2Deg, 0);
		}
	}
}
