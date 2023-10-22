using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Flocks.Jobs
{
	public struct GravityJob : IJobFor
	{
		private NativeArray<float3> _velocities;
		[ReadOnly] private readonly float3 _deltaGravity;

		public GravityJob(NativeArray<float3> velocities, float3 gravity, float deltaTime)
		{
			_velocities = velocities;
			_deltaGravity = gravity * deltaTime;
		}
		
		public void Execute(int index) => _velocities[index] += _deltaGravity;
	}
}