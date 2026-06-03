using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class TextVivoxSpeakerVolume : MonoBehaviour
    {
        public Text text;
        public TMPro.TextMeshProUGUI tmpText;
        public string textFormat = "{0} dB";
#if !UNITY_SERVER
        void Update()
        {
            string value = string.Format(textFormat, VivoxManager.Instance.SpeakerVolume);
            if (text != null)
                text.text = value;
            if (tmpText != null)
                tmpText.text = value;
        }
#endif
    }
}
