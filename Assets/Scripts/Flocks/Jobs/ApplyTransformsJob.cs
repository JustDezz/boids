using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Flocks.Jobs
{
	[BurstCompile]
	public struct ApplyTransformsJob : IJobParallelForTransform
	{
		private NativeArray<float3> _positions;
		[ReadOnly] private readonly NativeArray<float3> _velocities;
	
		[ReadOnly] private readonly float _deltaTime;
		[ReadOnly] private readonly float3 _minPosition;
		[ReadOnly] private readonly float3 _maxPosition;

		public ApplyTransformsJob(
			NativeArray<float3> positions, NativeArray<float3> velocities,
			float3 minPosition, float3 maxPosition, float deltaTime)
		{
			_maxPosition = maxPosition;
			_minPosition = minPosition;
			_positions = positions;
			_velocities = velocities;
			_deltaTime = deltaTime;
		}

		public void Execute(int index, TransformAccess transform)
		{
			float3 velocity = _velocities[index];
			float3 position = math.clamp(_positions[index] + velocity * _deltaTime, _minPosition, _maxPosition);
		
			float3 up = new(0, 1, 0);
			transform.position = position;
			transform.rotation = quaternion.LookRotation(math.normalize(velocity), up);
			_positions[index] = position;
		}
	}
}