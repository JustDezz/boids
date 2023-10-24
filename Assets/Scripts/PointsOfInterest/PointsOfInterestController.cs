using Tools.Extensions;
using Tools.Pool;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace PointsOfInterest
{
	[DefaultExecutionOrder(-5001)]
	public class PointsOfInterestController : MonoBehaviour
	{
		[SerializeField] private World _world;
		
		[Space]
		[SerializeField] private PointOfInterest[] _prefabs;
		[SerializeField] [Min(0)] private int _maxNumberOfPoints;
		[SerializeField] [Min(0)] private int _initialNumberOfPoints;
		[SerializeField] [Min(1)] private Vector2Int _spawnBatchSize = Vector2Int.one;
		[SerializeField] [Min(0)] private Vector2 _respawnTime;

		[Header("Spatial Hash Grid")]
		[SerializeField] [Min(0.01f)] private Vector3 _cellSize = Vector3.one;
		[SerializeField] [Min(0)] private float _gridRebuildDelay;
		
		private PointOfInterest[] _spawnedPois;
		private IPool<PointOfInterest> _poisPool;
		
		private NativeArray<PointOfInterestData> _pois;
		private SpatialHashGrid<int> _poisGrid;
		
		private bool _requireGridRebuild;
		private float _lastGridRebuild;
		private float _nextSpawn;
		private bool _isInitialized;
		
		public int NumberOfPoints { get; private set; }
		public NativeArray<PointOfInterestData> Pois => _pois;
		public SpatialHashGrid<int> PoisGrid => _poisGrid;

		private void Start() => Init();

		private void Init()
		{
			_poisPool ??= new MultiUnityPool<PointOfInterest>(_prefabs);

			if (_isInitialized)
			{
				Dispose();
				foreach (PointOfInterest poi in _spawnedPois) _poisPool.Return(poi);
			}

			if (_maxNumberOfPoints == 0) return;
			_spawnedPois = new PointOfInterest[_maxNumberOfPoints];
			_poisGrid = new SpatialHashGrid<int>(_world.HardBounds, _cellSize, _maxNumberOfPoints, Allocator.Persistent);
			_pois = new NativeArray<PointOfInterestData>(_maxNumberOfPoints, Allocator.Persistent);

			SpawnPoIs(_initialNumberOfPoints);

			_isInitialized = true;
		}

		private void Update()
		{
			float time = Time.time;
			if (_nextSpawn <= time) SpawnPoIs(_spawnBatchSize.GetRandom(true));
			if (_lastGridRebuild + _gridRebuildDelay <= time || _requireGridRebuild) RebuildGrid();
		}

		private void OnDestroy() => Dispose();

		private void Dispose()
		{
			_poisGrid.Dispose();
			_pois.Dispose();
		}

		private void RebuildGrid()
		{
#if ENABLE_PROFILER
			Profiler.BeginSample("Build PoIs spatial hash grid");
#endif
			_poisGrid.Clear();
			for (int i = 0; i < _spawnedPois.Length; i++)
			{
				PointOfInterest poi = _spawnedPois[i];
				if (poi == null) continue;
				Vector3 position = poi.transform.position;
				_poisGrid.Add(position, i);
			}
			_lastGridRebuild = Time.time;
			_requireGridRebuild = false;
#if ENABLE_PROFILER
			Profiler.EndSample();
#endif
		}

		public void SpawnPoIs(int count)
		{
			_nextSpawn = Time.time + _respawnTime.GetRandom();
			count = Mathf.Min(count, _maxNumberOfPoints - NumberOfPoints);
			if (count == 0) return;
			
			Bounds bounds = _world.SoftBounds;
			float3 min = bounds.min;
			float3 max = bounds.max;
			for (int i = 0; i < count; i++)
			{
				float3 normalizedPosition = new(Random.value, Random.value, Random.value);
				float3 position = math.lerp(min, max, normalizedPosition);

				PointOfInterest poi = _poisPool.Get();
				Transform poiTransform = poi.transform;
				poiTransform.SetParent(transform, false);
				poiTransform.position = position;
				
				poi.ResetUsages();
				_poisGrid.Add(position, NumberOfPoints);
				_spawnedPois[NumberOfPoints] = poi;
				_pois[NumberOfPoints] = new PointOfInterestData(position, poi.Usages);
				NumberOfPoints++;
			}

			_requireGridRebuild = true;
		}

		public void UpdateUsages()
		{
			for (int i = 0; i < NumberOfPoints; i++)
			{
				PointOfInterest poi = _spawnedPois[i];
				PointOfInterestData poiData = _pois[i];
				if (poi.Usages == poiData.Usages) continue;

				if (poiData.Usages > 0)
				{
					poi.Usages = poiData.Usages;
					continue;
				}

				_poisPool.Return(poi);
				int lastIndex = NumberOfPoints - 1;
				_spawnedPois[i] = _spawnedPois[lastIndex];
				_pois[i] = _pois[lastIndex];
				_spawnedPois[lastIndex] = default;
				_pois[lastIndex] = default;

				_requireGridRebuild = true;
				NumberOfPoints--;
				i--;
			}
		}

		private void OnValidate() => _initialNumberOfPoints = Mathf.Min(_initialNumberOfPoints, _maxNumberOfPoints);
	}
}