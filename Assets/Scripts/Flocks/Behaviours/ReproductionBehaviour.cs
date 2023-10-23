using Flocks.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Reproduction Behaviour")]
	public class ReproductionBehaviour : ScriptableFlockBehaviour
	{
		[SerializeField] [Min(0)] private float _reproductionDelay;
		[SerializeField] [Min(0)] private float _reproductionRadius;
		[SerializeField] [Min(0)] private int _maxReproductionsPerCall;

		private NativeArray<int2> _boidsIndices;
		private NativeArray<int> _breadedEntities;

		public override JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			if (timing != IFlockBehaviour.ScheduleTiming.AfterPositionsUpdate) return dependency;
			if (_maxReproductionsPerCall == 0)
			{
				Dispose();
				return dependency;
			}

			if (!_boidsIndices.IsCreated || _boidsIndices.Length != _maxReproductionsPerCall)
				_boidsIndices = new NativeArray<int2>(_maxReproductionsPerCall, Allocator.Persistent);
			if (!_breadedEntities.IsCreated)
				_breadedEntities = new NativeArray<int>(1, Allocator.Persistent);

			_breadedEntities[0] = 0;
			ReproductionJob job = new(
				flock.Boids, flock.BoidsGrid, _boidsIndices, _breadedEntities,
				_reproductionRadius, _reproductionDelay, Time.time);
			return job.Schedule(flock.NumberOfAgents, dependency);
		}

		private void OnDisable() => Dispose();
		private void OnDestroy() => Dispose();

		public override void OnFlockUpdated(Flock flock) => flock.Breed(_breadedEntities[0], _boidsIndices);

		private void Dispose()
		{
			if (!_boidsIndices.IsCreated) return;
			_boidsIndices.Dispose();
			_breadedEntities.Dispose();
		}
	}
}