using PlayerInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraManagement
{
	public class GameCamera : MonoBehaviour
	{
		[SerializeField] private World _world;

		[SerializeField] private Vector3 _rotation;
		[SerializeField] private Vector3 _rotationSpeed;
		[SerializeField] private Vector2 _distanceConstraint;
		[SerializeField] private float _smoothness;
		[SerializeField] private float _zoomSpeed;

		private float _targetDistance;
		
		private void Awake()
		{
			PlayerInputActions input = new();
			input.Camera.Zoom.performed += OnCameraZoom;
			input.Enable();

			Transform y = transform;
			y.rotation = Quaternion.Euler(_rotation);
			_targetDistance = _distanceConstraint.y;
			y.position = _world.HardBounds.center - y.forward * _targetDistance;
		}

		private void OnCameraZoom(InputAction.CallbackContext context)
		{
			float value = context.ReadValue<float>();
			float delta = value * _zoomSpeed * Time.deltaTime;
			_targetDistance = Mathf.Clamp(_targetDistance + delta, _distanceConstraint.x, _distanceConstraint.y);
		}

		private void LateUpdate()
		{
			Transform t = transform;
			Vector3 center = _world.HardBounds.center;
			
			float coefficient = 1 - Mathf.Exp(-_smoothness * Time.deltaTime);
			Quaternion rotation = Quaternion.Lerp(t.rotation, Quaternion.Euler(_rotation + _rotationSpeed * Time.time), coefficient);
			_targetDistance = Mathf.Clamp(_targetDistance, _distanceConstraint.x, _distanceConstraint.y);
			float distance = Mathf.Lerp((t.position - center).magnitude, _targetDistance, coefficient);
			Vector3 forward = rotation * Vector3.forward;
			Vector3 position = center - forward * distance;

			t.SetPositionAndRotation(position, rotation);
		}
	}
}