using TMPro;
using UnityEngine;

namespace Core.Localization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LangTmp: MonoBehaviour
    {
        public int key;
        private void Awake()
        {
            Localization.LanguageUpdate += Refresh;
            SetText();
        }

        private void Refresh()
        {
            SetText();
        }

        private void SetText()
        {
            if (key != 0)
            {
                string value = Localization.Load_UI(key);
                GetComponent<TextMeshProUGUI>().text = value;
            }
        }

        private void OnDestroy()
        {
            Localization.LanguageUpdate -= Refresh;
        }
    }
}