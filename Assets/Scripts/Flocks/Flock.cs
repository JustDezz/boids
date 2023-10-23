using System.Linq;
using Flocks.Behaviours;
using Flocks.Jobs;
using Tools.Pool;
using Tools.RestrictTypeAttribute;
using Tools.UnwrapNestingAttribute;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Flocks
{
	[DefaultExecutionOrder(-5000)]
	public class Flock : MonoBehaviour
	{
		[SerializeField] private World _world;

		[Space]
		[SerializeField] private GameObject[] _boidsPrefabs;
		[SerializeField] private FlockSettings _flockSettings;
		[SerializeField] [Min(0)] private int _numberOfAgents;
		[SerializeField] [Min(0)] private float _density = 0.1f;
		
		[Space]
		[SerializeField] [Restrict(typeof(IFlockBehaviour))] [Unwrap] private Object[] _behaviours;
		
		[Header("Spatial Hash Grid")]
		[SerializeField] [Min(0.01f)] private Vector3 _cellSize = Vector3.one;
		[SerializeField] [Min(0)] private float _gridRebuildDelay;

		private IPool<Transform> _boidsPool;
		private Transform[] _boids;
		
		private NativeArray<float3> _positions;
		private NativeArray<float3> _velocities;
		private SpatialHashGrid<int> _boidsGrid;
		private TransformAccessArray _transformAccessArray;

		private JobHandle _jobHandle;
		private float _lastGridRebuildTime;

		public int NumberOfAgents => _numberOfAgents;
		public FlockSettings FlockSettings => _flockSettings;
		public NativeArray<float3> Positions => _positions;
		public NativeArray<float3> Velocities => _velocities;
		public SpatialHashGrid<int> BoidsGrid => _boidsGrid;
		
		public Bounds SoftBounds => _world.SoftBounds;
		public Bounds HardBounds => _world.HardBounds;

		private void Start() => InitAgents();

		private void InitAgents()
		{
			if (_boids != null)
			{
				Dispose();
				foreach (Transform t in _boids) _boidsPool.Return(t);
			}

			_boidsPool ??= new MultiUnityPool<Transform>(_boidsPrefabs.Select(p => p.transform));

			_boidsGrid = new SpatialHashGrid<int>(HardBounds, _cellSize, _numberOfAgents, Allocator.Persistent);
			_positions = new NativeArray<float3>(_numberOfAgents, Allocator.Persistent);
			_velocities = new NativeArray<float3>(_numberOfAgents, Allocator.Persistent);
			_boids = new Transform[_numberOfAgents];
		
			float spawnRadius = _numberOfAgents * _density;
			float2 speed = _flockSettings.Speed;
			for (int i = 0; i < _numberOfAgents; i++)
			{
				Vector3 position = Random.insideUnitSphere * spawnRadius;
				Vector3 direction = Random.onUnitSphere;
				Vector3 velocity = direction * Random.Range(speed.x, speed.y);

				Transform boid = _boidsPool.Get();
				// Boids have to have different root objects to be properly parallelized 
				// on multiple cores in IJobParallelForTransform
				boid.SetParent(null, false);
				boid.SetPositionAndRotation(position, Quaternion.LookRotation(direction));

				_positions[i] = position;
				_velocities[i] = velocity;
				_boids[i] = boid.transform;
				_boidsGrid.Add(position, i);
			}

			_transformAccessArray = new TransformAccessArray(_boids);
			_lastGridRebuildTime = Time.time;
		}

		private void Update()
		{
			if (_lastGridRebuildTime + _gridRebuildDelay < Time.time) RebuildHashGrid();

			JobHandle handle = default;
			foreach (Object item in _behaviours)
			{
				IFlockBehaviour behaviour = (IFlockBehaviour) item;
				handle = behaviour.Schedule(this, handle);
				behaviour.OnBeforeFlockUpdate();
			}

			ApplyTransformsJob transformsJob = new(_positions, _velocities, HardBounds, Time.deltaTime);
			handle = transformsJob.Schedule(_transformAccessArray, handle);

			_jobHandle = handle;
		}

		private void RebuildHashGrid()
		{
#if ENABLE_PROFILER
			Profiler.BeginSample("Build boids spatial hash grid");
#endif
			_boidsGrid.Clear();
			for (int i = 0; i < _positions.Length; i++) _boidsGrid.Add(_positions[i], i);
			_lastGridRebuildTime = Time.time;
#if ENABLE_PROFILER
			Profiler.EndSample();
#endif
		}

		private void LateUpdate()
		{
			_jobHandle.Complete();
			foreach (Object item in _behaviours)
			{
				IFlockBehaviour behaviour = (IFlockBehaviour) item;
				behaviour.OnFlockUpdated();
			}
		}

		private void OnDestroy() => Dispose();

		private void Dispose()
		{
			_positions.Dispose();
			_velocities.Dispose();
			_boidsGrid.Dispose();
			_transformAccessArray.Dispose();
		}

		private void OnValidate()
		{
			_flockSettings.Validate();
		
			if (!Application.isPlaying || Time.frameCount < 1) return;
			if (_boids.Length == _numberOfAgents) return;
			InitAgents();
		}
	}
}
