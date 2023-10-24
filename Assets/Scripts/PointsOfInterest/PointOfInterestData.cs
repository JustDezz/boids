using Unity.Mathematics;

namespace PointsOfInterest
{
	public struct PointOfInterestData
	{
		public float3 Position;
		public int Usages;

		public PointOfInterestData(float3 position, int usages)
		{
			Position = position;
			Usages = usages;
		}
	}
}