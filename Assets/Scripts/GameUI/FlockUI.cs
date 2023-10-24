using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameUI
{
	public class FlockUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text _boidsCount;
		[SerializeField] private TMP_Text _fps;
		[SerializeField] private RectTransform _parent;
		[SerializeField] private FlockSettingsGroup _groupPrefab;

		private List<FlockSettingsGroup> _groups;
		
		public static FlockUI Instance { get; private set; }

		private void Awake() => Instance = this;

		public FlockSettingsGroup AddGroup()
		{
			_groups ??= new List<FlockSettingsGroup>();
			FlockSettingsGroup group = Instantiate(_groupPrefab, _parent);
			_groups.Add(group);
			return group;
		}

		public void UpdateBoidsCount(int count)
		{
			_boidsCount.text = $"Boids: {count}";
			_fps.text = $"FPS {1 / Time.deltaTime: #00.0}";
		}

		public void Clear()
		{
			if (_groups == null) return;
			foreach (FlockSettingsGroup group in _groups) Destroy(group.gameObject);
			_groups.Clear();
		}
	}
}