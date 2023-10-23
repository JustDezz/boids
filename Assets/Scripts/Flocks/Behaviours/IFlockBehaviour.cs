using Unity.Jobs;

namespace Flocks.Behaviours
{
	public interface IFlockBehaviour
	{
		public JobHandle Schedule(Flock flock, JobHandle dependency = default);
	}
}