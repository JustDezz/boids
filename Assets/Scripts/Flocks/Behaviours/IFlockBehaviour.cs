using Unity.Jobs;

namespace Flocks.Behaviours
{
	public interface IFlockBehaviour
	{
		public JobHandle Schedule(Flock flock, ScheduleTiming timing, JobHandle dependency = default);
		public void OnFlockUpdated(Flock flock);
		
		public enum ScheduleTiming
		{
			BeforePositionsUpdate,
			AfterPositionsUpdate
		}
	}
}