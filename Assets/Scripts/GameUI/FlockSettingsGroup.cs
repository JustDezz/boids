using GameUI.Elements;
using TMPro;
using UnityEngine;

namespace GameUI
{
	public class FlockSettingsGroup : MonoBehaviour
	{
		[SerializeField] private TMP_Text _groupName;
		[SerializeField] private RectTransform _parent;

		[SerializeField] private CustomButton _buttonPrefab;
		[SerializeField] private SliderWithInputField _sliderPrefab;
		[SerializeField] private VectorField _vectorPrefab;

		public void SetName(string groupName) => _groupName.text = groupName;
		public CustomButton AddButton() => Instantiate(_buttonPrefab, _parent);
		public CustomButton AddButton(string label)
		{
			CustomButton button = Instantiate(_buttonPrefab, _parent);
			button.SetText(label);
			return button;
		}

		public SliderWithInputField AddSlider() => Instantiate(_sliderPrefab, _parent);
		public SliderWithInputField AddSlider(string label, float min, float max, float value)
		{
			SliderWithInputField slider = Instantiate(_sliderPrefab, _parent);
			slider.SetLabel(label);
			slider.SetValues(min, max, value);
			return slider;
		}

		public VectorField AddVectorField() => Instantiate(_vectorPrefab, _parent);
		public VectorField AddVectorField(string label, int components)
		{
			VectorField field = AddVectorField();
			field.SetLabel(label);
			field.Components = components;
			return field;
		}
		public VectorField AddVectorField(string label, int components, Vector4 value)
		{
			VectorField field = AddVectorField();
			field.SetLabel(label);
			field.SetValue(value);
			field.Components = components;
			return field;
		}
		public VectorField AddVectorField(string label, int components, Vector4 min, Vector4 max, Vector4 value)
		{
			VectorField field = AddVectorField();
			field.SetLabel(label);
			field.SetValues(min, max, value);
			field.Components = components;
			return field;
		}
	}
}