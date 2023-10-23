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
		private Transform[] _spawnedBoids;
		
		private NativeArray<BoidData> _boids;
		private SpatialHashGrid<int> _boidsGrid;
		private TransformAccessArray _transformAccessArray;

		private JobHandle _jobHandle;
		private float _lastGridRebuildTime;

		public int NumberOfAgents => _numberOfAgents;
		public FlockSettings FlockSettings => _flockSettings;
		public NativeArray<BoidData> Boids => _boids;
		public SpatialHashGrid<int> BoidsGrid => _boidsGrid;
		
		public Bounds SoftBounds => _world.SoftBounds;
		public Bounds HardBounds => _world.HardBounds;

		private void Start() => InitAgents();

		private void InitAgents()
		{
			if (_spawnedBoids != null)
			{
				Dispose();
				foreach (Transform t in _spawnedBoids) _boidsPool.Return(t);
			}

			_boidsPool ??= new MultiUnityPool<Transform>(_boidsPrefabs.Select(p => p.transform));

			_boidsGrid = new SpatialHashGrid<int>(HardBounds, _cellSize, _numberOfAgents, Allocator.Persistent);
			_boids = new NativeArray<BoidData>(_numberOfAgents, Allocator.Persistent);
			_spawnedBoids = new Transform[_numberOfAgents];
		
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

				_boids[i] = new BoidData(position, velocity);
				_spawnedBoids[i] = boid.transform;
				_boidsGrid.Add(position, i);
			}

			_transformAccessArray = new TransformAccessArray(_spawnedBoids);
			_lastGridRebuildTime = Time.time;
		}

		private void Update()
		{
			if (_lastGridRebuildTime + _gridRebuildDelay <= Time.time) RebuildHashGrid();

			ApplyTransformsJob transformsJob = new(_boids, HardBounds, Time.deltaTime);
			JobHandle handle = ScheduleBehaviours(IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate, default);
			handle = transformsJob.Schedule(_transformAccessArray, handle);
			handle = ScheduleBehaviours(IFlockBehaviour.ScheduleTiming.AfterPositionsUpdate, handle);

			_jobHandle = handle;
		}

		private JobHandle ScheduleBehaviours(IFlockBehaviour.ScheduleTiming timing, JobHandle dependency)
		{
			foreach (Object item in _behaviours)
			{
				IFlockBehaviour behaviour = (IFlockBehaviour) item;
				dependency = behaviour.Schedule(this, timing, dependency);
			}
			
			return dependency;
		}

		private void RebuildHashGrid()
		{
#if ENABLE_PROFILER
			Profiler.BeginSample("Build boids spatial hash grid");
#endif
			_boidsGrid.Clear();
			for (int i = 0; i < _numberOfAgents; i++) _boidsGrid.Add(_boids[i].Position, i);
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
			_boids.Dispose();
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
