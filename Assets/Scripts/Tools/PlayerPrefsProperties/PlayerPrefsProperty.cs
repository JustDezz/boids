using System;

namespace Tools.PlayerPrefsProperties
{
	public abstract class PlayerPrefsProperty<T>
	{
		protected abstract Func<string, T, T> Getter { get; }
		protected abstract Action<string, T> Setter { get; }

		public T DefaultValue { get; }
		public string Key { get; }
		
		protected PlayerPrefsProperty(string key)
		{
			Key = key;
			DefaultValue = default;
		}

		protected PlayerPrefsProperty(string key, T defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}
		protected void Init() => _value = Getter(Key, DefaultValue);

		private T _value;
		public T Value
		{
			get => _value;
			set
			{
				_value = value;
				Setter(Key, value);
			}
		}
	}
}