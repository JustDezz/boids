using System;
using System.Linq;
using Flocks.Behaviours;
using Flocks.Jobs;
using GameUI;
using Tools.Pool;
using Tools.RestrictTypeAttribute;
using Tools.UnwrapNestingAttribute;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
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
		[SerializeField] [Min(0)] private int _maxNumberOfAgents;
		[SerializeField] [Min(0)] private int _initialNumberOfAgents;
		[SerializeField] [Min(0)] private float _density = 0.1f;
		
		[Space]
		[SerializeField] [Restrict(typeof(IFlockBehaviour))] [Unwrap] private Object[] _behaviours;
		
		[Header("Spatial Hash Grid")]
		[SerializeField] [Min(0.01f)] private Vector3 _cellSize = Vector3.one;
		[SerializeField] [Min(0)] private float _gridRebuildDelay;

		private IPool<Transform> _boidsPool;
		private Transform[] _spawnedBoids;
		private bool _initialized;
		
		private NativeArray<BoidData> _boids;
		private SpatialHashGrid<int> _boidsGrid;
		private TransformAccessArray _transformAccessArray;

		private JobHandle _jobHandle;
		private float _lastGridRebuildTime;

		public int NumberOfAgents { get; private set; }
		public FlockSettings FlockSettings => _flockSettings;
		public NativeArray<BoidData> Boids => _boids;
		public SpatialHashGrid<int> BoidsGrid => _boidsGrid;
		
		public Bounds SoftBounds => _world.SoftBounds;
		public Bounds HardBounds => _world.HardBounds;

		private void Start() => InitAgents();

		private void InitAgents()
		{
			if (_initialized && _boids.Length != _maxNumberOfAgents) Reinitialize();
			else
			{
				_boidsPool = new MultiUnityPool<Transform>(_boidsPrefabs.Select(p => p.transform));
				_boidsGrid = new SpatialHashGrid<int>(HardBounds, _cellSize, _maxNumberOfAgents, Allocator.Persistent);
				_boids = new NativeArray<BoidData>(_maxNumberOfAgents, Allocator.Persistent);
				_spawnedBoids = new Transform[_maxNumberOfAgents];
				
			}

			// Very quick and dirty solution just to get job done
			CreateUI();
			_initialized = true;

			int toSpawn = _initialNumberOfAgents - NumberOfAgents;
			if (toSpawn <= 0) return;
			
			float spawnRadius = toSpawn * _density;
			float2 speedLimit = _flockSettings.Speed;
			for (int i = 0; i < toSpawn; i++)
			{
				Vector3 position = Random.insideUnitSphere * spawnRadius;
				Vector3 direction = Random.onUnitSphere;
				float speed = Random.Range(speedLimit.x, speedLimit.y);

				SpawnBoid(position, direction, speed);
			}
			_lastGridRebuildTime = Time.time;
			_transformAccessArray = new TransformAccessArray(_spawnedBoids);
		}

		private void CreateUI()
		{
			FlockSettingsUI.Instance.Clear();
			foreach (Object behaviour in _behaviours)
			{
				if (behaviour is IAdjustableBehaviour adjustableBehaviour)
					adjustableBehaviour.CreateUI(FlockSettingsUI.Instance);
			}
		}

		private void Reinitialize()
		{
			int previousLength = _boids.Length;

			int toDestroy = NumberOfAgents - _maxNumberOfAgents;
			for (int i = 0; i < toDestroy; i++) _boidsPool.Return(_spawnedBoids[--NumberOfAgents]);

			NativeArray<BoidData> boids = new(_maxNumberOfAgents, Allocator.Persistent);
			NativeArray<BoidData>.Copy(_boids, boids, Mathf.Min(previousLength, _maxNumberOfAgents));
			Transform[] spawnedBoids = new Transform[_maxNumberOfAgents];
			Array.Copy(_spawnedBoids, spawnedBoids, _maxNumberOfAgents);

			_boids.Dispose();
			_boidsGrid.Dispose();
			_transformAccessArray.Dispose();

			_boidsGrid = new SpatialHashGrid<int>(HardBounds, _cellSize, _maxNumberOfAgents, Allocator.Persistent);
			_boids = boids;
			_spawnedBoids = spawnedBoids;
			_transformAccessArray = new TransformAccessArray(_spawnedBoids);
		}

		private void SpawnBoid(Vector3 position, Vector3 direction, float speed)
		{
			Transform boid = _boidsPool.Get();
			// Boids have to have different root objects for the IJobParallelForTransform
			// to be properly parallelized properly with multiple threads
			boid.SetParent(null, false);
			boid.SetPositionAndRotation(position, Quaternion.LookRotation(direction));

			_boids[NumberOfAgents] = new BoidData(position, direction * speed);
			_spawnedBoids[NumberOfAgents] = boid.transform;
			_boidsGrid.Add(position, NumberOfAgents);
			NumberOfAgents++;
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
		private void LateUpdate()
		{
			_jobHandle.Complete();
			foreach (Object item in _behaviours)
			{
				IFlockBehaviour behaviour = (IFlockBehaviour) item;
				behaviour.OnFlockUpdated(this);
			}
		}
		private void OnDestroy() => Dispose();

		public void Breed(int count, NativeArray<int2> indices)
		{
			count = Mathf.Min(count, _maxNumberOfAgents - NumberOfAgents);
			if (count == 0) return;
			
			for (int i = 0; i < count; i++)
			{
				int2 index = indices[i];
				BoidData first = _boids[index.x];
				BoidData second = _boids[index.y];
				Vector3 position = (first.Position + second.Position) / 2;
				Vector3 velocity = (first.Velocity + second.Velocity) / 2;
				float speed = velocity.magnitude;
				Vector3 direction = velocity / speed;

				SpawnBoid(position, direction, speed);
			}
			_transformAccessArray.SetTransforms(_spawnedBoids);
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
			for (int i = 0; i < NumberOfAgents; i++) _boidsGrid.Add(_boids[i].Position, i);
			_lastGridRebuildTime = Time.time;
#if ENABLE_PROFILER
			Profiler.EndSample();
#endif
		}

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
			if (_boids.Length == _maxNumberOfAgents) return;
			InitAgents();
		}
	}
}
