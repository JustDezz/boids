using Flocks.Behaviours;
using Flocks.Jobs;
using Tools.GizmosExtensions;
using Tools.UnwrapNestingAttribute;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

namespace Flocks
{
	[DefaultExecutionOrder(-5000)]
	public class Flock : MonoBehaviour
	{
		[SerializeField] private GameObject _boidPrefab;
		[SerializeField] [Unwrap] private FlockBehaviour[] _behaviours;

		[Space]
		[SerializeField] [Min(0)] private int _numberOfAgents;
		[SerializeField] [Min(0)] private float _density = 0.1f;
		[SerializeField] private FlockSettings _flockSettings;
	
		[Space]
		[SerializeField] private Bounds _softBounds;
		[SerializeField] private Bounds _hardBounds;

		private NativeArray<float3> _positions;
		private NativeArray<float3> _velocities;
		private TransformAccessArray _transformAccessArray;
		private Transform[] _transforms;
		private JobHandle _boidsHandle;

		public int NumberOfAgents => _numberOfAgents;
		public FlockSettings FlockSettings => _flockSettings;
		public NativeArray<float3> Positions => _positions;
		public NativeArray<float3> Velocities => _velocities;

		public Bounds SoftBounds => _softBounds;
		public Bounds HardBounds => _hardBounds;

		private void Start() => InitAgents();

		private void InitAgents()
		{
			if (_transforms != null)
			{
				Dispose();
				foreach (Transform t in _transforms) Destroy(t.gameObject);
			}

			_positions = new NativeArray<float3>(_numberOfAgents, Allocator.Persistent);
			_velocities = new NativeArray<float3>(_numberOfAgents, Allocator.Persistent);
			_transforms = new Transform[_numberOfAgents];
		
			float spawnRadius = _numberOfAgents * _density;
			float2 speed = _flockSettings.Speed;
			for (int i = 0; i < _numberOfAgents; i++)
			{
				Vector3 position = Random.insideUnitSphere * spawnRadius;
				Vector3 direction = Random.onUnitSphere;

				GameObject boid = Instantiate(_boidPrefab, position, Quaternion.LookRotation(direction), transform);
				_positions[i] = position;
				_velocities[i] = direction * Random.Range(speed.x, speed.y);
				_transforms[i] = boid.transform;
			}

			_transformAccessArray = new TransformAccessArray(_transforms);
		}

		private void Update()
		{
			JobHandle handle = default;
			foreach (FlockBehaviour behaviour in _behaviours) handle = behaviour.Schedule(this, handle);
			ApplyTransformsJob transformsJob = new(_positions, _velocities, _hardBounds.min, _hardBounds.max, Time.deltaTime);
			handle = transformsJob.Schedule(_transformAccessArray, handle);

			_boidsHandle = handle;
		}

		private void LateUpdate() => _boidsHandle.Complete();

		private void OnDestroy() => Dispose();

		private void Dispose()
		{
			_positions.Dispose();
			_velocities.Dispose();
			_transformAccessArray.Dispose();
		}

		private void OnValidate()
		{
			_flockSettings.Validate();
			Vector3 hardBoundsMin = Vector3.Min(_hardBounds.min, _softBounds.min);
			Vector3 hardBoundsMax = Vector3.Max(_hardBounds.max, _softBounds.max);
			_hardBounds.SetMinMax(hardBoundsMin, hardBoundsMax);
		
			if (!Application.isPlaying || Time.frameCount < 1) return;
			if (_transforms.Length == _numberOfAgents) return;
			InitAgents();
		}

		private void OnDrawGizmos()
		{
			using (new GizmosColorScope(Color.blue)) Gizmos.DrawWireCube(_softBounds.center, _softBounds.size);
			using (new GizmosColorScope(Color.red)) Gizmos.DrawWireCube(_hardBounds.center, _hardBounds.size);
		}
	}
}
