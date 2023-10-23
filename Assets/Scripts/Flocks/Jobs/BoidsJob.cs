using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Jobs
{
	[BurstCompile]
	public struct BoidsJob : IJobParallelFor
	{
		// As the boids are independent from one another in their nature, 
		// the race conditions doesn't affect anything, thus disabling 
		// parallel for restriction allows us to gain extra performance
		// by using IJobParallelFor instead of IJobFor
		[NativeDisableParallelForRestriction] private NativeArray<BoidData> _data;
		
		[ReadOnly] private SpatialHashGrid<int> _boidsGrid;
	
		[ReadOnly] private readonly float _fov;
		[ReadOnly] private readonly float _radius;
		[ReadOnly] private readonly float _avoidRadius;
		[ReadOnly] private readonly float2 _speed;
		[ReadOnly] private readonly float _deltaTurn;
	
		[ReadOnly] private readonly float3 _minPosition;
		[ReadOnly] private readonly float3 _maxPosition;
	
		[ReadOnly] private readonly float _avoidanceFactor;
		[ReadOnly] private readonly float _alignmentFactor;
		[ReadOnly] private readonly float _cohesionFactor;

		public BoidsJob(
			NativeArray<BoidData> data, SpatialHashGrid<int> boidsGrid,
			FlockSettings flockSettings, Bounds bounds, float deltaTime,
			float avoidanceFactor, float alignmentFactor, float cohesionFactor)
		{
			_data = data;
			_boidsGrid = boidsGrid;
		
			_fov = flockSettings.FOV;
			_radius = flockSettings.InfluenceRadius;
			_avoidRadius = flockSettings.AvoidRadius;
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
			BoidData data = _data[index];
			float3 position = data.Position;
			float3 velocity = data.Velocity;
			float3 direction = math.normalize(velocity);

			float sqrRadius = _radius * _radius;
			float sqrAvoidRadius = _avoidRadius * _avoidRadius;

			int consideredNeighbours = 0;
			int avoidedNeighbours = 0;
			float3 avoidanceVector = float3.zero;
			float3 alignmentVector = float3.zero;
			float3 centerOfMass = float3.zero;

			using SpatialHashGrid<int>.AreaEnumerator enumerator = _boidsGrid.GetEnumerator(position, _radius);
			while (enumerator.MoveNext())
			{
				int i = enumerator.Current;
				if (i == index) continue;

				BoidData otherData = _data[i];
				float3 otherPosition = otherData.Position;
				float3 offset = otherPosition - position;
				float sqrDistance = math.lengthsq(offset);
				if (sqrDistance > sqrRadius || sqrDistance == 0) continue;

				float distance = math.sqrt(sqrDistance);
				float3 offsetDirection = offset / distance;
				float dot = math.dot(direction, offsetDirection);
				if (dot < _fov) continue;

				if (sqrDistance <= sqrAvoidRadius)
				{
					avoidedNeighbours++;
					avoidanceVector -= offset;
				}

				consideredNeighbours++;
				alignmentVector += otherData.Velocity;
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
			
			data.Velocity = velocity / speed * clampedSpeed;
			_data[index] = data;
		}
	}
}