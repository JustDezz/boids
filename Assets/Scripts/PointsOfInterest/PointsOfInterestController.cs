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
		
		private PointOfInterest[] _pois;
		private IPool<PointOfInterest> _poisPool;
		
		private NativeArray<PointOfInterestData> _data;
		private SpatialHashGrid<int> _poisGrid;
		
		private bool _requireGridRebuild;
		private float _lastGridRebuild;
		private float _nextSpawn;
		private bool _isInitialized;
		
		public int NumberOfPoints { get; private set; }
		public NativeArray<PointOfInterestData> Data => _data;
		public SpatialHashGrid<int> PoisGrid => _poisGrid;

		private void Start() => Init();

		private void Init()
		{
			_poisPool ??= new MultiUnityPool<PointOfInterest>(_prefabs);

			if (_isInitialized)
			{
				Dispose();
				foreach (PointOfInterest poi in _pois) _poisPool.Return(poi);
			}

			if (_maxNumberOfPoints == 0) return;
			_pois = new PointOfInterest[_maxNumberOfPoints];
			_poisGrid = new SpatialHashGrid<int>(_world.HardBounds, _cellSize, _maxNumberOfPoints, Allocator.Persistent);
			_data = new NativeArray<PointOfInterestData>(_maxNumberOfPoints, Allocator.Persistent);

			SpawnBatch(_initialNumberOfPoints);

			_isInitialized = true;
		}

		private void Update()
		{
			float time = Time.time;
			if (_nextSpawn <= time) SpawnBatch(_spawnBatchSize.GetRandom(true));
			if (_lastGridRebuild + _gridRebuildDelay <= time || _requireGridRebuild) RebuildGrid();
		}

		private void OnDestroy() => Dispose();

		private void Dispose()
		{
			_poisGrid.Dispose();
			_data.Dispose();
		}

		private void RebuildGrid()
		{
#if ENABLE_PROFILER
			Profiler.BeginSample("Build PoIs spatial hash grid");
#endif
			_poisGrid.Clear();
			for (int i = 0; i < _pois.Length; i++)
			{
				PointOfInterest poi = _pois[i];
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

		private void SpawnBatch(int batchSize)
		{
			_nextSpawn = Time.time + _respawnTime.GetRandom();
			batchSize = Mathf.Min(batchSize, _maxNumberOfPoints - NumberOfPoints);
			if (batchSize == 0) return;
			
			Bounds bounds = _world.SoftBounds;
			float3 min = bounds.min;
			float3 max = bounds.max;
			for (int i = 0; i < batchSize; i++)
			{
				float3 normalizedPosition = new(Random.value, Random.value, Random.value);
				float3 position = math.lerp(min, max, normalizedPosition);

				PointOfInterest poi = _poisPool.Get();
				Transform poiTransform = poi.transform;
				poiTransform.SetParent(transform, false);
				poiTransform.position = position;
				
				poi.ResetUsages();
				_poisGrid.Add(position, NumberOfPoints);
				_pois[NumberOfPoints] = poi;
				_data[NumberOfPoints] = new PointOfInterestData(position, poi.Usages);
				NumberOfPoints++;
			}

			_requireGridRebuild = true;
		}

		public void UpdateUsages()
		{
			for (int i = 0; i < NumberOfPoints; i++)
			{
				PointOfInterest poi = _pois[i];
				PointOfInterestData poiData = _data[i];
				if (poi.Usages == poiData.Usages) continue;

				if (poiData.Usages > 0)
				{
					poi.Usages = poiData.Usages;
					continue;
				}

				_poisPool.Return(poi);
				int lastIndex = NumberOfPoints - 1;
				_pois[i] = _pois[lastIndex];
				_data[i] = _data[lastIndex];
				_pois[lastIndex] = default;
				_data[lastIndex] = default;

				_requireGridRebuild = true;
				NumberOfPoints--;
				i--;
			}
		}

		private void OnValidate() => _initialNumberOfPoints = Mathf.Min(_initialNumberOfPoints, _maxNumberOfPoints);
	}
}