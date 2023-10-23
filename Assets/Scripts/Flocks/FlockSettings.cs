using System;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks
{
	[Serializable]
	public struct FlockSettings
	{
		[SerializeField] [Range(-1, 1)] private float _fov;
		
		[SerializeField] private float2 _speed;
		[SerializeField] private float _turnSpeed;
		
		[SerializeField] [Min(0)] private float _influenceRadius;
		[SerializeField] [Min(0)] private float _avoidRadius;
		[SerializeField] [Min(0)] private float _poiRadius;
		
		public float FOV => _fov;

		public float2 Speed => _speed;
		public float TurnSpeed => _turnSpeed;
		
		public float InfluenceRadius => _influenceRadius;
		public float AvoidRadius => _avoidRadius;
		public float PoIRadius => _poiRadius;

		public void Validate() => _avoidRadius = Mathf.Clamp(_avoidRadius, 0, _influenceRadius);
	}
}