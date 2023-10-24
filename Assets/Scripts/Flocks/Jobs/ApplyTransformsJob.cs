using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Flocks.Jobs
{
	[BurstCompile]
	public struct ApplyTransformsJob : IJobParallelForTransform
	{
		private NativeArray<BoidData> _data;
	
		[ReadOnly] private readonly float _deltaTime;
		[ReadOnly] private readonly float3 _minPosition;
		[ReadOnly] private readonly float3 _maxPosition;

		public ApplyTransformsJob(NativeArray<BoidData> data, Bounds bounds, float deltaTime)
		{
			_data = data;
			_maxPosition = bounds.max;
			_minPosition = bounds.min;
			_deltaTime = deltaTime;
		}

		public void Execute(int index, TransformAccess transform)
		{
			BoidData data = _data[index];
			float3 velocity = data.Velocity;
			float3 position = data.Position + velocity * _deltaTime;
			float3 clampedPosition = math.clamp(position, _minPosition, _maxPosition);
		
			float3 up = new(0, 1, 0);
			transform.position = clampedPosition;
			transform.rotation = quaternion.LookRotation(math.normalize(velocity), up);

			data.Position = clampedPosition;
			_data[index] = data;
		}
	}
}