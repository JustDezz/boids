using Unity.Mathematics;

namespace Flocks
{
	public struct BoidData
	{
		public float3 Position;
		public float3 Velocity;
		
		public bool HasConsumedFood;
		public float LastReproduction;

		public BoidData(float3 position, float3 velocity) : this()
		{
			Position = position;
			Velocity = velocity;
		}
	}
}