using System;
using UnityEngine;

namespace Tools.PlayerPrefsProperties
{
	public class PlayerPrefsFloatProperty : PlayerPrefsProperty<float>
	{
		protected override Func<string, float, float> Getter { get; } = PlayerPrefs.GetFloat;
		protected override Action<string, float> Setter { get; } = PlayerPrefs.SetFloat;
		
		public PlayerPrefsFloatProperty(string key) : base(key) => Init();
		public PlayerPrefsFloatProperty(string key, float defaultValue) : base(key, defaultValue) => Init();
		
		public static implicit operator float(PlayerPrefsFloatProperty property) => property.Value;
	}
}