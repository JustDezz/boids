using System;
using UnityEngine;

namespace Tools.PlayerPrefsProperties
{
	public class PlayerPrefsStringProperty : PlayerPrefsProperty<string>
	{
		protected override Func<string, string, string> Getter { get; } = PlayerPrefs.GetString;
		protected override Action<string, string> Setter { get; } = PlayerPrefs.SetString;

		public PlayerPrefsStringProperty(string key) : base(key) => Init();
		public PlayerPrefsStringProperty(string key, string defaultValue) : base(key, defaultValue) => Init();

		public static implicit operator string(PlayerPrefsStringProperty property) => property.Value;
	}
}