using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Flocks.Jobs
{
	[BurstCompile]
	public struct ReproductionJob : IJobFor
	{
		private NativeArray<BoidData> _boids;
		[ReadOnly] private SpatialHashGrid<int> _boidsGrid;
		[WriteOnly] private NativeArray<int2> _boidsIndices;
		private NativeArray<int> _breadedEntities;
		
		[ReadOnly] private readonly float _radius;
		[ReadOnly] private readonly float _maxSpawnTime;
		[ReadOnly] private readonly float _time;

		public ReproductionJob(NativeArray<BoidData> boids, SpatialHashGrid<int> boidsGrid,
			NativeArray<int2> boidsIndices, NativeArray<int> breadedEntities,
			float radius, float delay, float time)
		{
			_breadedEntities = breadedEntities;
			_boidsIndices = boidsIndices;
			_boids = boids;
			_boidsGrid = boidsGrid;
			_radius = radius;
			_time = time;
			_maxSpawnTime = _time - delay;
		}

		public void Execute(int index)
		{
			BoidData boid = _boids[index];
			if (!CanBreed(boid)) return;

			using SpatialHashGrid<int>.AreaEnumerator areaEnumerator = _boidsGrid.GetEnumerator(boid.Position, _radius);
			while (areaEnumerator.MoveNext())
			{
				int otherIndex = areaEnumerator.Current;
				if (otherIndex == index) continue;

				BoidData otherBoid = _boids[otherIndex];
				if (!CanBreed(otherBoid)) continue;

				int breadedCount = _breadedEntities[0];
				_boidsIndices[breadedCount] = new int2(index, otherIndex);
				boid.HasConsumedFood = false;
				boid.LastReproduction = _time;
				_boids[index] = boid;

				otherBoid.HasConsumedFood = false;
				otherBoid.LastReproduction = _time;
				_boids[otherIndex] = otherBoid;
				_breadedEntities[0] = breadedCount + 1;

				return;
			}
		}

		private bool CanBreed(BoidData boid)
		{
			if (!boid.HasConsumedFood) return false;
			return boid.LastReproduction <= _maxSpawnTime;
		}
	}
}