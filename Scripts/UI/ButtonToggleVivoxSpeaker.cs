using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class ButtonToggleVivoxSpeaker : MonoBehaviour
    {
        public Button button;
#if !UNITY_SERVER
        private void Awake()
        {
            if (button == null)
                button = GetComponentInChildren<Button>();
            button.onClick.AddListener(OnClick);
            VivoxManager_OnCurrentInitializeStateChanged();
            VivoxManager.OnCurrentInitializeStateChanged += VivoxManager_OnCurrentInitializeStateChanged;
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
            VivoxManager.OnCurrentInitializeStateChanged -= VivoxManager_OnCurrentInitializeStateChanged;
        }

        private void OnClick()
        {
            VivoxManager.Instance.ToggleSpeaker();
        }

        private void VivoxManager_OnCurrentInitializeStateChanged()
        {
            bool initialized = VivoxManager.CurrentInitializeState == VivoxManager.InitializeState.Initialized;
            button.interactable = initialized;
        }
#endif
    }
}
