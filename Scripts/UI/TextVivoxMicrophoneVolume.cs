using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class TextVivoxMicrophoneVolume : MonoBehaviour
    {
        public Text text;
        public TMPro.TextMeshPro tmpText;

        void Update()
        {
            string value = VivoxManager.Instance.MicrophoneVolume.ToString("N0");
            if (text != null)
                text.text = value;
            if (tmpText != null)
                tmpText.text = value;
        }
    }
}
