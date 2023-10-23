using Flocks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PointsOfInterest
{
	[BurstCompile]
	public struct VisitPointsOfInterestJob : IJobParallelFor
	{
		[ReadOnly] private readonly NativeArray<float3> _boidsPositions;
		
		private NativeArray<float3> _boidsVelocities;
		
		[ReadOnly] private readonly float _poiRadius;
		[ReadOnly] private readonly NativeArray<PointOfInterestData> _pois;
		[ReadOnly] private readonly float2 _poiInfluence;
		[ReadOnly] private readonly float _deltaTime;
		
		[ReadOnly] private SpatialHashGrid<int> _poisGrid;

		public VisitPointsOfInterestJob(
			NativeArray<float3> boidsPositions, NativeArray<float3> boidsVelocities, FlockSettings flockSettings,
			NativeArray<PointOfInterestData> pois, SpatialHashGrid<int> poisGrid,
			float2 poiInfluence, float deltaTime)
		{
			_deltaTime = deltaTime;
			_poiInfluence = poiInfluence;
			_pois = pois;
			_poisGrid = poisGrid;
			_boidsVelocities = boidsVelocities;
			_boidsPositions = boidsPositions;

			_poiRadius = flockSettings.PoIRadius;
		}
		
		public void Execute(int index)
		{
			float3 position = _boidsPositions[index];
			float3 velocity = _boidsVelocities[index];
			
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

			_boidsVelocities[index] = direction * speed;
		}
	}
}