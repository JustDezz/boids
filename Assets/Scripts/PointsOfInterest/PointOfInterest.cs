using UnityEngine;

namespace PointsOfInterest
{
	public class PointOfInterest : MonoBehaviour
	{
		[SerializeField] private int _maxUsages;
		
		private int _usages;

		public bool IsDepleted { get; private set; }

		public int Usages
		{
			get => _usages;
			set
			{
				_usages = value;
				transform.localScale = Vector3.one * Mathf.InverseLerp(0, _maxUsages, _usages);
				IsDepleted = _usages <= 0;
			}
		}

		public void ResetUsages() => Usages = _maxUsages;
	}
}