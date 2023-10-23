using Tools.GizmosExtensions;
using UnityEngine;

public class World : MonoBehaviour
{
	[SerializeField] private Bounds _softBounds = new(Vector3.zero, Vector3.one);
	[SerializeField] private Bounds _hardBounds = new(Vector3.zero, Vector3.one);

	public Bounds SoftBounds => _softBounds;
	public Bounds HardBounds => _hardBounds;

	private void OnValidate()
	{
		Vector3 hardBoundsMin = Vector3.Min(_hardBounds.min, _softBounds.min);
		Vector3 hardBoundsMax = Vector3.Max(_hardBounds.max, _softBounds.max);
		_hardBounds.SetMinMax(hardBoundsMin, hardBoundsMax);
	}

	private void OnDrawGizmos()
	{
		using (new GizmosColorScope(Color.blue)) Gizmos.DrawWireCube(_softBounds.center, _softBounds.size);
		using (new GizmosColorScope(Color.red)) Gizmos.DrawWireCube(_hardBounds.center, _hardBounds.size);
	}
}