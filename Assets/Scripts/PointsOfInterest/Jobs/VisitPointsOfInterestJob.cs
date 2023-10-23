using Flocks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PointsOfInterest.Jobs
{
	[BurstCompile]
	public struct VisitPointsOfInterestJob : IJobParallelFor
	{
		private NativeArray<BoidData> _boids;
		
		[ReadOnly] private readonly NativeArray<PointOfInterestData> _pois;
		[ReadOnly] private SpatialHashGrid<int> _poisGrid;

		[ReadOnly] private readonly float _poiRadius;
		[ReadOnly] private readonly float2 _poiInfluence;
		[ReadOnly] private readonly float _deltaTime;

		public VisitPointsOfInterestJob(
			NativeArray<BoidData> boids, FlockSettings flockSettings,
			NativeArray<PointOfInterestData> pois, SpatialHashGrid<int> poisGrid,
			float2 poiInfluence, float deltaTime)
		{
			_boids = boids;
			_pois = pois;
			_poisGrid = poisGrid;
			
			_poiRadius = flockSettings.PoIRadius;
			_poiInfluence = poiInfluence;
			_deltaTime = deltaTime;
		}
		
		public void Execute(int index)
		{
			BoidData boid = _boids[index];
			float3 position = boid.Position;
			float3 velocity = boid.Velocity;
			
			float speed = math.length(velocity);
			float3 direction = velocity / speed;
			float sqrPoIRadius = _poiRadius * _poiRadius;
			
			using SpatialHashGrid<int>.AreaEnumerator areaEnumerator = _poisGrid.GetEnumerator(position, _poiRadius);
			while (areaEnumerator.MoveNext())
			{
				int poiIndex = areaEnumerator.Current;
				PointOfInterestData poi = _pois[poiIndex];
				if (poi.Usages <= 0) continue;
				
				float3 offset = poi.Position - position;
				float sqrDistance = math.lengthsq(offset);
				if (sqrDistance > sqrPoIRadius || sqrDistance == 0) continue;
				
				float distance = math.sqrt(sqrDistance);
				float3 offsetDirection = offset / distance;

				float influence = math.lerp(_poiInfluence.x, _poiInfluence.y, distance / _poiRadius);
				direction = math.lerp(direction, offsetDirection, influence * _deltaTime);
				direction = math.normalize(direction);
			}

			boid.Velocity = direction * speed;
			_boids[index] = boid;
		}
	}
}