using Flocks.Jobs;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Gravity Behaviour")]
	public class GravityBehaviour : ScriptableFlockBehaviour
	{
		[SerializeField] private float3 _gravity;

		public override JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			if (timing != IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate) return dependency;
			if (math.all(_gravity == float3.zero)) return dependency;
			GravityJob job = new(flock.Boids, _gravity, Time.deltaTime);
			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}
	}
}