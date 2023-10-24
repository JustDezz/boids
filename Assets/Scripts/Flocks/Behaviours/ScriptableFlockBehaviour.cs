using Unity.Jobs;
using UnityEngine;

namespace Flocks.Behaviours
{
	public abstract class ScriptableFlockBehaviour : ScriptableObject, IFlockBehaviour
	{
		protected const string DataPath = "Data/Flock/"; 

		public abstract JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default);
		public virtual void OnFlockUpdated(Flock flock) { }
	}
}