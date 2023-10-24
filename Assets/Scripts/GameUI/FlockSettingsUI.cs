using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameUI
{
	public class FlockSettingsUI : MonoBehaviour
	{
		[SerializeField] private RectTransform _parent;
		[SerializeField] private FlockSettingsGroup _groupPrefab;

		private List<FlockSettingsGroup> _groups;
		
		public static FlockSettingsUI Instance { get; private set; }

		private void Awake() => Instance = this;

		public FlockSettingsGroup AddGroup()
		{
			_groups ??= new List<FlockSettingsGroup>();
			FlockSettingsGroup group = Instantiate(_groupPrefab, _parent);
			_groups.Add(group);
			return group;
		}

		public void Clear()
		{
			if (_groups == null) return;
			foreach (FlockSettingsGroup group in _groups) Destroy(group.gameObject);
			_groups.Clear();
		}
	}
}