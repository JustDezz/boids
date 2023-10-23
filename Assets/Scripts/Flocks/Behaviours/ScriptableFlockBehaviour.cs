using Unity.Jobs;
using UnityEngine;

namespace Flocks.Behaviours
{
	public abstract class ScriptableFlockBehaviour : ScriptableObject, IFlockBehaviour
	{
#if UNITY_EDITOR
		protected const string DataPath = "Data/Flock/"; 
#endif
		
		public abstract JobHandle Schedule(Flock flock, JobHandle dependency = default);
	}
}