using Unity.Jobs;

namespace Flocks.Behaviours
{
	public interface IFlockBehaviour
	{
		public void OnBeforeFlockUpdate();
		public JobHandle Schedule(Flock flock, JobHandle dependency = default);
		public void OnFlockUpdated();
	}
}