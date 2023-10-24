using System;
using Flocks.Jobs;
using GameUI;
using GameUI.Elements;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Gravity Behaviour")]
	public class GravityBehaviour : ScriptableFlockBehaviour, IAdjustableBehaviour
	{
		[SerializeField] private float3 _gravity;

		public float3 Gravity { get; set; }

		public override JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			if (timing != IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate) return dependency;
			if (math.all(Gravity == float3.zero)) return dependency;
			GravityJob job = new(flock.Boids, Gravity, Time.deltaTime);
			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}

		private void OnEnable() => Gravity = _gravity;
		private void OnValidate() => Gravity = _gravity;

		public void CreateUI(FlockSettingsUI ui)
		{
			FlockSettingsGroup group = ui.AddGroup();
			group.SetName("Gravity");
			VectorField vectorField = group.AddVectorField();
			vectorField.SetLabel("Gravity");
			vectorField.Components = 3;
			vectorField.SetValue((Vector3) _gravity);
			vectorField.ValueChanged += v => Gravity = (Vector3) v;
		}
	}
}