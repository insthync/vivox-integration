using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class ButtonToggleVivoxSpeaker : MonoBehaviour
    {
#if !UNITY_SERVER
        public Button button;

        private void Awake()
        {
            if (button == null)
                button = GetComponentInChildren<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            VivoxManager.Instance.ToggleSpeaker();
        }
#endif
    }
}
