using Flocks;
using Flocks.Behaviours;
using PointsOfInterest.Jobs;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PointsOfInterest.Behaviours
{
	public class VisitPointsOfInterestBehaviour : MonoBehaviour, IFlockBehaviour
	{
		[SerializeField] private PointsOfInterestController _controller;
		[SerializeField] private float2 _pointInfluence;
		[SerializeField] [Min(0)] private float _pointConsumeRadius;
		
		public void OnBeforeFlockUpdate() { }

		public JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			dependency = timing switch
			{
				IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate => ScheduleVisitJob(flock, dependency),
				IFlockBehaviour.ScheduleTiming.AfterPositionsUpdate => ScheduleDepleteJob(flock, dependency),
				_ => dependency
			};

			return dependency;
		}

		private JobHandle ScheduleVisitJob(Flock flock, JobHandle dependency)
		{
			if (math.all(_pointInfluence == float2.zero)) return dependency;
			
			VisitPointsOfInterestJob visitJob = new(
				flock.Boids, flock.FlockSettings,
				_controller.Pois, _controller.PoisGrid,
				_pointInfluence, Time.deltaTime);

			return visitJob.Schedule(flock.NumberOfAgents, 0, dependency);
		}

		private JobHandle ScheduleDepleteJob(Flock flock, JobHandle dependency)
		{
			if (_pointConsumeRadius == 0) return dependency;
			
			DepletePointsOfInterest depleteJob = new(flock.Boids, flock.BoidsGrid, _controller.Pois, _pointConsumeRadius);
			return depleteJob.Schedule(_controller.NumberOfPoints, dependency);
		}

		public void OnFlockUpdated(Flock flock) => _controller.UpdateUsages();
	}
}