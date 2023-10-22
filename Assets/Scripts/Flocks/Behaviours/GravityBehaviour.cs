using Flocks.Jobs;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Gravity Behaviour")]
	public class GravityBehaviour : FlockBehaviour
	{
		[SerializeField] private float3 _gravity;

		public override JobHandle Schedule(Flock flock, JobHandle dependency = default)
		{
			if (math.all(_gravity == float3.zero)) return dependency;
			GravityJob job = new(flock.Velocities, _gravity, Time.deltaTime);
			return job.Schedule(flock.NumberOfAgents, dependency);
		}
	}
}