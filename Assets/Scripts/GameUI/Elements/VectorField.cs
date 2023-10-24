using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace GameUI.Elements
{
	public class VectorField : MonoBehaviour
	{
		public event Action<Vector4> ValueChanged; 
		
		[SerializeField] private TMP_Text _label;
		[SerializeField] private TMP_InputField[] _inputFields;
		
		private int _components;
		private Vector4 _min;
		private Vector4 _max;
		private Vector4 _current;

		public int Components
		{
			get => _components;
			set
			{
				_components = Mathf.Clamp(value, 1, 4);
				for (int i = 0; i < 4; i++) _inputFields[i].gameObject.SetActive(i < _components);
			}
		}

		private void Awake()
		{
			for (int i = 0; i < 4; i++)
			{
				TMP_InputField inputField = _inputFields[i];
				inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
				inputField.onSubmit.AddListener(OnSubmit);
			}
		}

		private void OnSubmit(string _)
		{
			bool changed = false;
			for (int i = 0; i < 4; i++)
			{
				TMP_InputField field = _inputFields[i];
				string text = field.text;
				if (float.TryParse(text, out float result))
				{
					_current[i] = Mathf.Clamp(result, _min[i], _max[i]);
					changed = true;
				}
				else field.text = _current[i].ToString(CultureInfo.InvariantCulture);
			}
			if (changed) ValueChanged?.Invoke(_current);
		}

		public void SetLabel(string label) => _label.text = label;
		public void SetValue(Vector4 value) => SetValues(Vector4.negativeInfinity, Vector4.positiveInfinity, value);
		public void SetValues(Vector4 min, Vector4 max, Vector4 current)
		{
			_min = min;
			_max = max;
			_current = current;
			for (int i = 0; i < 4; i++) _inputFields[i].text = _current[i].ToString(CultureInfo.InvariantCulture);
		}
	}
}