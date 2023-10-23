using Flocks.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Boids Behaviour")]
	public class BoidsBehaviour : ScriptableFlockBehaviour
	{
		[SerializeField] [Range(0, 1)] private float _avoidanceFactor;
		[SerializeField] [Range(0, 1)] private float _alignmentFactor;
		[SerializeField] [Range(0, 1)] private float _cohesionFactor;
		
		public override JobHandle Schedule(Flock flock, JobHandle dependency = default)
		{
			NativeArray<float3> positions = flock.Positions;
			NativeArray<float3> velocities = flock.Velocities;
			SpatialHashGrid<int> grid = flock.BoidsGrid;
			FlockSettings settings = flock.FlockSettings;
			Bounds softBounds = flock.SoftBounds;
			BoidsJob job = new(
				positions, velocities, grid,
				settings, softBounds, Time.deltaTime, 
				_avoidanceFactor, _alignmentFactor, _cohesionFactor);
			
			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}
	}
}