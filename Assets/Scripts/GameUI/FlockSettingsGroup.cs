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
		public SliderWithInputField AddSlider() => Instantiate(_sliderPrefab, _parent);
		public VectorField AddVectorField() => Instantiate(_vectorPrefab, _parent);
	}
}