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
		private float AvoidanceRadius { get; set; }
		private Vector2 Speed { get; set; }

		public override JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			if (timing != IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate) return dependency;
			
			SpatialHashGrid<int> grid = flock.BoidsGrid;
			FlockSettings settings = flock.FlockSettings;
			settings.Speed = Speed;
			settings.AvoidRadius = AvoidanceRadius;
			Bounds softBounds = flock.SoftBounds;
			BoidsJob job = new(
				flock.Boids, grid,
				settings, softBounds, Time.deltaTime, 
				AvoidanceFactor, AlignmentFactor, CohesionFactor);
			
			return job.Schedule(flock.NumberOfAgents, 0, dependency);
		}

		public void CreateUI(Flock flock, FlockUI ui)
		{
			FlockSettingsGroup group = ui.AddGroup();
			group.SetName("General");
			group.AddSlider("Avoidance", 0, 1, _avoidanceFactor).ValueChanged += v => AvoidanceFactor = v;
			group.AddSlider("Alignment", 0, 1, _alignmentFactor).ValueChanged += v => AlignmentFactor = v;
			group.AddSlider("Cohesion", 0, 1, _cohesionFactor).ValueChanged += v => CohesionFactor = v;
			
			FlockSettings settings = flock.FlockSettings;
			AvoidanceRadius = settings.AvoidRadius;
			Speed = settings.Speed;
			
			Vector4 speed = settings.Speed.xyxy;
			VectorField vectorField = group.AddVectorField("Speed limits", 2, speed);
			vectorField.ValueChanged += v => Speed = new Vector2(v.x, v.y);
			group.AddSlider("Avoidance Radius", 0, 0.5f, AvoidanceRadius).ValueChanged += v => AvoidanceRadius = v;
		}

		private void OnEnable() => WriteDefaults();
		private void OnValidate() => WriteDefaults();

		private void WriteDefaults()
		{
			AvoidanceFactor = _avoidanceFactor;
			AlignmentFactor = _alignmentFactor;
			CohesionFactor = _cohesionFactor;
		}
	}
}