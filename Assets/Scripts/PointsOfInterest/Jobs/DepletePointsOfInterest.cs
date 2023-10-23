using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PointsOfInterest.Jobs
{
	[BurstCompile]
	public struct DepletePointsOfInterest : IJobParallelFor
	{
		[ReadOnly] private SpatialHashGrid<int> _boidsGrid;
		private NativeArray<PointOfInterestData> _data;
		[ReadOnly] private readonly NativeArray<float3> _positions;
		[ReadOnly] private readonly float _consumeRadius;

		public DepletePointsOfInterest(
			NativeArray<float3> positions, SpatialHashGrid<int> boidsGrid,
			NativeArray<PointOfInterestData> data, float consumeRadius)
		{
			_positions = positions;
			_boidsGrid = boidsGrid;
			_data = data;
			_consumeRadius = consumeRadius;
		}

		public void Execute(int index)
		{
			PointOfInterestData data = _data[index];
			float3 position = data.Position;
			int usages = data.Usages;
			if (usages <= 0) return;

			float sqrRadius = _consumeRadius * _consumeRadius;
			using SpatialHashGrid<int>.AreaEnumerator areaEnumerator = _boidsGrid.GetEnumerator(position, _consumeRadius);
			while (areaEnumerator.MoveNext())
			{
				int boidIndex = areaEnumerator.Current;
				float3 boidPosition = _positions[boidIndex];
				float3 offset = boidPosition - position;
				float sqrDistance = math.lengthsq(offset);
				
				if (sqrDistance > sqrRadius) continue;
				usages--;
				if (usages <= 0) break;
			}

			data.Usages = usages;
			_data[index] = data;
		}
	}
}