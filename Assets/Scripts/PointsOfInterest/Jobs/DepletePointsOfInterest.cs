using Flocks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PointsOfInterest.Jobs
{
	[BurstCompile]
	public struct DepletePointsOfInterest : IJobFor
	{
		// For some reason Unity throws exception when trying to read from this array,
		// as if it was used from multiple threads like in IJobParallelFor (even though
		// this job is executed on single thread), so sticking this attribute solves the issue
		[NativeDisableParallelForRestriction] private NativeArray<BoidData> _boids;
		[ReadOnly] private SpatialHashGrid<int> _boidsGrid;
		private NativeArray<PointOfInterestData> _pois;

		[ReadOnly] private readonly float _consumeRadius;
		
		public DepletePointsOfInterest(
			NativeArray<BoidData> boids, SpatialHashGrid<int> boidsGrid,
			NativeArray<PointOfInterestData> pois, float consumeRadius)
		{
			_boids = boids;
			_boidsGrid = boidsGrid;
			_pois = pois;
			_consumeRadius = consumeRadius;
		}

		public void Execute(int index)
		{
			PointOfInterestData data = _pois[index];
			float3 position = data.Position;
			int usages = data.Usages;
			if (usages <= 0) return;

			float sqrRadius = _consumeRadius * _consumeRadius;
			using SpatialHashGrid<int>.AreaEnumerator areaEnumerator = _boidsGrid.GetEnumerator(position, _consumeRadius);
			while (areaEnumerator.MoveNext())
			{
				int boidIndex = areaEnumerator.Current;
				BoidData boid = _boids[boidIndex];
				if (boid.HasConsumedFood) continue;
				
				float3 boidPosition = boid.Position;
				float3 offset = boidPosition - position;
				float sqrDistance = math.lengthsq(offset);
				
				if (sqrDistance > sqrRadius) continue;
				boid.HasConsumedFood = true;
				_boids[boidIndex] = boid;
				usages--;
				if (usages <= 0) break;
			}

			data.Usages = usages;
			_pois[index] = data;
		}
	}
}