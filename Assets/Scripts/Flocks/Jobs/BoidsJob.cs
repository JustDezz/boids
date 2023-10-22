using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Jobs
{
	[BurstCompile]
	public struct BoidsJob : IJobFor
	{
		[ReadOnly] private readonly NativeArray<float3> _positions;
		private NativeArray<float3> _velocities;
	
		[ReadOnly] private readonly float _fov;
		[ReadOnly] private readonly float _sqrRadius;
		[ReadOnly] private readonly float _sqrAvoidRadius;
		[ReadOnly] private readonly float2 _speed;
		[ReadOnly] private readonly float _deltaTurn;
	
		[ReadOnly] private readonly float3 _minPosition;
		[ReadOnly] private readonly float3 _maxPosition;
	
		[ReadOnly] private readonly float _avoidanceFactor;
		[ReadOnly] private readonly float _alignmentFactor;
		[ReadOnly] private readonly float _cohesionFactor;

		public BoidsJob(NativeArray<float3> positions, NativeArray<float3> velocities,
			FlockSettings flockSettings, Bounds bounds, float deltaTime, 
			float avoidanceFactor, float alignmentFactor, float cohesionFactor)
		{
			_velocities = velocities;
			_positions = positions;
		
			_fov = flockSettings.FOV;
			_sqrRadius = flockSettings.InfluenceRadius * flockSettings.InfluenceRadius;
			_sqrAvoidRadius = flockSettings.AvoidRadius * flockSettings.AvoidRadius;
			_speed = flockSettings.Speed;
			_deltaTurn = flockSettings.TurnSpeed * deltaTime;
		
			_minPosition = bounds.min;
			_maxPosition = bounds.max;

			_avoidanceFactor = avoidanceFactor;
			_alignmentFactor = alignmentFactor;
			_cohesionFactor = cohesionFactor;
		}

		public void Execute(int index)
		{
			float3 position = _positions[index];
			float3 velocity = _velocities[index];
			float3 direction = math.normalize(velocity);
		
			int agentsCount = _velocities.Length;

			int consideredNeighbours = 0;
			int avoidedNeighbours = 0;
			float3 avoidanceVector = float3.zero;
			float3 alignmentVector = float3.zero;
			float3 centerOfMass = float3.zero;
		
			for (int i = 0; i < agentsCount; i++)
			{
				if (i == index) continue;
			
				float3 otherPosition = _positions[i];
				float3 offset = otherPosition - position;
				float sqrDistance = math.lengthsq(offset);
				if (sqrDistance > _sqrRadius) continue;

				float distance = math.sqrt(sqrDistance);
				float3 offsetDirection = offset / distance;
				float dot = math.dot(direction, offsetDirection);
				if (dot < _fov) continue;
			
				if (sqrDistance <= _sqrAvoidRadius)
				{
					avoidedNeighbours++;
					avoidanceVector -= offset;
				}

				consideredNeighbours++;
				alignmentVector += _velocities[i];
				centerOfMass += otherPosition;
			}

			if (consideredNeighbours > 0)
			{
				if (avoidedNeighbours > 0) velocity += avoidanceVector / avoidedNeighbours * _avoidanceFactor;
				velocity += (alignmentVector / consideredNeighbours - velocity) * _alignmentFactor;
				velocity += (centerOfMass / consideredNeighbours - position) * _cohesionFactor;
			}

			velocity += new float3(
				position.x <= _minPosition.x ? _deltaTurn : 0,
				position.y <= _minPosition.y ? _deltaTurn : 0,
				position.z <= _minPosition.z ? _deltaTurn : 0);

			velocity += new float3(
				position.x >= _maxPosition.x ? -_deltaTurn : 0,
				position.y >= _maxPosition.y ? -_deltaTurn : 0,
				position.z >= _maxPosition.z ? -_deltaTurn : 0);

			float speed = math.length(velocity);
			float clampedSpeed = math.clamp(speed, _speed.x, _speed.y);
			_velocities[index] = velocity / speed * clampedSpeed;
		}
	}
}