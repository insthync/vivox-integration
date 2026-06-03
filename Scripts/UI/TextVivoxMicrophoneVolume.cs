using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class TextVivoxMicrophoneVolume : MonoBehaviour
    {
        public Text text;
        public TMPro.TextMeshPro tmpText;
        public string textFormat = "{0} dB";

        void Update()
        {
            string value = string.Format(textFormat, VivoxManager.Instance.MicrophoneVolume);
            if (text != null)
                text.text = value;
            if (tmpText != null)
                tmpText.text = value;
        }
    }
}
