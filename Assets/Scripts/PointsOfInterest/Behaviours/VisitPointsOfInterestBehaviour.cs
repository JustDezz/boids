using Cysharp.Threading.Tasks;
using Flocks;
using Flocks.Behaviours;
using GameUI;
using GameUI.Elements;
using PointsOfInterest.Jobs;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PointsOfInterest.Behaviours
{
	public class VisitPointsOfInterestBehaviour : MonoBehaviour, IFlockBehaviour, IAdjustableBehaviour
	{
		[SerializeField] private PointsOfInterestController _controller;
		[SerializeField] private float2 _pointInfluence;
		[SerializeField] [Min(0)] private float _pointConsumeRadius;
		[SerializeField] [Min(1)] private int _spawnFoodByButton = 1;

		private float PointConsumeRadius { get; set; }
		private float2 PointInfluence { get; set; }

		public void OnBeforeFlockUpdate() { }

		public JobHandle Schedule(Flock flock, IFlockBehaviour.ScheduleTiming timing, JobHandle dependency = default)
		{
			dependency = timing switch
			{
				IFlockBehaviour.ScheduleTiming.BeforePositionsUpdate => ScheduleVisitJob(flock, dependency),
				IFlockBehaviour.ScheduleTiming.AfterPositionsUpdate => ScheduleDepleteJob(flock, dependency),
				_ => dependency
			};

			return dependency;
		}

		public void OnFlockUpdated(Flock flock) => _controller.UpdateUsages();
		public void CreateUI(Flock flock, FlockUI ui)
		{
			FlockSettingsGroup group = ui.AddGroup();
			group.SetName("Points of interest");
			SliderWithInputField slider = group.AddSlider("Consummation radius", 0.1f, 1f, PointConsumeRadius);
			slider.ValueChanged += v => PointConsumeRadius = v;

			VectorField vector = group.AddVectorField("Point influence by distance", 2, _pointInfluence.xyxy);
			vector.ValueChanged += v => PointInfluence = new float2(v.x, v.y);

			CustomButton button = group.AddButton($"Add {_spawnFoodByButton} food");
			button.OnClick.AddListener(() => SpawnPoIsDelayed().Forget());
		}

		private async UniTaskVoid SpawnPoIsDelayed()
		{
			await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: this.GetCancellationTokenOnDestroy());
			_controller.SpawnPoIs(_spawnFoodByButton);
		}

		private void OnEnable() => WriteDefaults();
		private void OnValidate() => WriteDefaults();

		private void WriteDefaults()
		{
			PointConsumeRadius = _pointConsumeRadius;
			PointInfluence = _pointInfluence;
		}

		private JobHandle ScheduleVisitJob(Flock flock, JobHandle dependency)
		{
			if (math.all(PointInfluence == float2.zero)) return dependency;
			
			VisitPointsOfInterestJob visitJob = new(
				flock.Boids, flock.FlockSettings,
				_controller.Pois, _controller.PoisGrid,
				PointInfluence, Time.deltaTime);

			return visitJob.Schedule(flock.NumberOfAgents, 0, dependency);
		}

		private JobHandle ScheduleDepleteJob(Flock flock, JobHandle dependency)
		{
			if (PointConsumeRadius == 0) return dependency;
			
			DepletePointsOfInterest depleteJob = new(flock.Boids, flock.BoidsGrid, _controller.Pois, PointConsumeRadius);
			return depleteJob.Schedule(_controller.NumberOfPoints, dependency);
		}
	}
}