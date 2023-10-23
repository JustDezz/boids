using Flocks;
using Flocks.Behaviours;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PointsOfInterest
{
	public class VisitPointsOfInterestBehaviour : MonoBehaviour, IFlockBehaviour
	{
		[SerializeField] private PointsOfInterestController _controller;
		[SerializeField] private float2 _pointInfluence;
		
		public JobHandle Schedule(Flock flock, JobHandle dependency = default)
		{
			if (math.all(_pointInfluence == float2.zero)) return dependency;
			
			VisitPointsOfInterestJob job = new(
				flock.Positions, flock.Velocities, flock.FlockSettings,
				_controller.Data, _controller.PoisGrid,
				_pointInfluence, Time.deltaTime);

			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}
	}
}