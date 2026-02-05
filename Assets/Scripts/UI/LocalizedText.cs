using UnityEngine;
using TMPro;
using ComBoom.Core;

namespace ComBoom.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        public string localizationKey;

        private TextMeshProUGUI textComponent;

        private void OnEnable()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= UpdateText;
        }

        private void UpdateText()
        {
            if (textComponent != null && !string.IsNullOrEmpty(localizationKey))
                textComponent.text = LocalizationManager.Get(localizationKey);
        }
    }
}
