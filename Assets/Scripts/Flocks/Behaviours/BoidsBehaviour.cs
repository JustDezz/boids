using Flocks.Jobs;
using Unity.Jobs;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Boids Behaviour")]
	public class BoidsBehaviour : ScriptableFlockBehaviour
	{
		[SerializeField] [Range(0, 1)] private float _avoidanceFactor;
		[SerializeField] [Range(0, 1)] private float _alignmentFactor;
		[SerializeField] [Range(0, 1)] private float _cohesionFactor;
		
		public override JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			if (timing != IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate) return dependency;
			
			SpatialHashGrid<int> grid = flock.BoidsGrid;
			FlockSettings settings = flock.FlockSettings;
			Bounds softBounds = flock.SoftBounds;
			BoidsJob job = new(
				flock.Boids, grid,
				settings, softBounds, Time.deltaTime, 
				_avoidanceFactor, _alignmentFactor, _cohesionFactor);
			
			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}
	}
}