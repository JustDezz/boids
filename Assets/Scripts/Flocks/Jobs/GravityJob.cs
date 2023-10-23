using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Flocks.Jobs
{
	public struct GravityJob : IJobParallelFor
	{
		private NativeArray<BoidData> _data;
		[ReadOnly] private readonly float3 _deltaGravity;

		public GravityJob(NativeArray<BoidData> data, float3 gravity, float deltaTime)
		{
			_data = data;
			_deltaGravity = gravity * deltaTime;
		}
		
		public void Execute(int index)
		{
			BoidData data = _data[index];
			data.Velocity += _deltaGravity;
			_data[index] = data;
		}
	}
}