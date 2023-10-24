using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI.Elements
{
	public class SliderWithInputField : MonoBehaviour
	{
		public event Action<float> ValueChanged;
		
		[SerializeField] private Slider _slider;
		[SerializeField] private TMP_InputField _inputField;
		[SerializeField] private TMP_Text _label;
		
		public bool WholeNumbers
		{
			get => _slider.wholeNumbers;
			set => _slider.wholeNumbers = value;
		}

		private void Awake()
		{
			_slider.onValueChanged.AddListener(OnSliderValueChanged);
			_inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
			_inputField.onSubmit.AddListener(OnSubmit);
		}

		public void SetLabel(string label) => _label.text = label;

		private void OnSubmit(string text)
		{
			if (float.TryParse(text, out float value))
			{
				_slider.SetValueWithoutNotify(value);
				ValueChanged?.Invoke(value);
			}
			else UpdateText();
		}

		private void UpdateText() => _inputField.text = _slider.value.ToString(CultureInfo.InvariantCulture);

		private void OnSliderValueChanged(float value)
		{
			ValueChanged?.Invoke(value);
			UpdateText();
		}

		public void SetValues(float min, float max, float current)
		{
			_slider.minValue = min;
			_slider.maxValue = max;
			_slider.SetValueWithoutNotify(current);
			
			UpdateText();
		}
	}
}