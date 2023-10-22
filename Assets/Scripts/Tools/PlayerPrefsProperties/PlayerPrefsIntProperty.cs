using System;
using UnityEngine;

namespace Tools.PlayerPrefsProperties
{
	public class PlayerPrefsIntProperty : PlayerPrefsProperty<int>
	{
		protected override Func<string, int, int> Getter { get; } = PlayerPrefs.GetInt;
		protected override Action<string, int> Setter { get; } = PlayerPrefs.SetInt;

		public PlayerPrefsIntProperty(string key) : base(key) => Init();
		public PlayerPrefsIntProperty(string key, int defaultValue) : base(key, defaultValue) => Init();

		public static implicit operator int(PlayerPrefsIntProperty property) => property.Value;
	}
}