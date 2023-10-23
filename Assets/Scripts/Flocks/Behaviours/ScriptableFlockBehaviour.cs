using Unity.Jobs;
using UnityEngine;

namespace Flocks.Behaviours
{
	public abstract class ScriptableFlockBehaviour : ScriptableObject, IFlockBehaviour
	{
#if UNITY_EDITOR
		protected const string DataPath = "Data/Flock/"; 
#endif

		public virtual void OnBeforeFlockUpdate() { }
		public abstract JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default);
		public virtual void OnFlockUpdated() { }
	}
}