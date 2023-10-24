using Flocks.Jobs;
using GameUI;
using GameUI.Elements;
using Unity.Jobs;
using UnityEngine;

namespace Flocks.Behaviours
{
	[CreateAssetMenu(menuName = DataPath + "Boids Behaviour")]
	public class BoidsBehaviour : ScriptableFlockBehaviour, IAdjustableBehaviour
	{
		[SerializeField] [Range(0, 1)] private float _avoidanceFactor;
		[SerializeField] [Range(0, 1)] private float _alignmentFactor;
		[SerializeField] [Range(0, 1)] private float _cohesionFactor;

		private float AvoidanceFactor { get; set; }
		private float AlignmentFactor { get; set; }
		private float CohesionFactor { get; set; }

		public override JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			if (timing != IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate) return dependency;
			
			SpatialHashGrid<int> grid = flock.BoidsGrid;
			FlockSettings settings = flock.FlockSettings;
			Bounds softBounds = flock.SoftBounds;
			BoidsJob job = new(
				flock.Boids, grid,
				settings, softBounds, Time.deltaTime, 
				AvoidanceFactor, AlignmentFactor, CohesionFactor);
			
			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}

		public void CreateUI(FlockSettingsUI ui)
		{
			FlockSettingsGroup group = ui.AddGroup();
			group.SetName("General");
			AddSlider(group, "Avoidance", _avoidanceFactor).ValueChanged += v => AvoidanceFactor = v;
			AddSlider(group, "Alignment", _alignmentFactor).ValueChanged += v => AlignmentFactor = v;
			AddSlider(group, "Cohesion", _cohesionFactor).ValueChanged += v => CohesionFactor = v;
		}

		private void OnEnable() => WriteDefaults();
		private void OnValidate() => WriteDefaults();

		private void WriteDefaults()
		{
			AvoidanceFactor = _avoidanceFactor;
			AlignmentFactor = _alignmentFactor;
			CohesionFactor = _cohesionFactor;
		}

		private static SliderWithInputField AddSlider(FlockSettingsGroup group, string label, float value)
		{
			SliderWithInputField slider = group.AddSlider();
			slider.SetLabel(label);
			slider.SetValues(0, 1, value);
			return slider;
		}
	}
}