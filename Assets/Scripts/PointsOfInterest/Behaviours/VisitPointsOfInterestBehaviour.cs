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
		
		private JobHandle _depleteHandle;

		public void OnBeforeFlockUpdate() { }

		public JobHandle Schedule(Flock flock, JobHandle dependency = default)
		{
			if (math.all(_pointInfluence == float2.zero)) return dependency;
			
			VisitPointsOfInterestJob visitJob = new(
				flock.Positions, flock.Velocities, flock.FlockSettings,
				_controller.Data, _controller.PoisGrid,
				_pointInfluence, Time.deltaTime);

			JobHandle jobHandle = visitJob.Schedule(flock.NumberOfAgents, 0, dependency);

			DepletePointsOfInterest depleteJob = new(flock.Positions, flock.BoidsGrid, _controller.Data, _pointConsumeRadius);
			_depleteHandle = depleteJob.Schedule(_controller.NumberOfPoints, 0, jobHandle);

			return _depleteHandle;
		}

		public void OnFlockUpdated() => _controller.UpdateUsages();
	}
}