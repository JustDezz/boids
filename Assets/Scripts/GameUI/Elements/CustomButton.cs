using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI.Elements
{
	public class CustomButton : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private TMP_Text _text;

		public Button.ButtonClickedEvent OnClick => _button.onClick;
		public void SetText(string text) => _text.text = text;
	}
}