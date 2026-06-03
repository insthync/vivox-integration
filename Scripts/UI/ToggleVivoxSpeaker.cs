using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class ToggleVivoxSpeaker : MonoBehaviour
    {
#if !UNITY_SERVER
        public Toggle toggle;

        private void Awake()
        {
            if (toggle == null)
                toggle = GetComponentInChildren<Toggle>();
            toggle.onValueChanged.AddListener(OnToggle);
            VivoxManager_OnCurrentInitializeStateChanged();
            VivoxManager.OnCurrentInitializeStateChanged += VivoxManager_OnCurrentInitializeStateChanged;
        }

        private void OnEnable()
        {
            toggle.SetIsOnWithoutNotify(!VivoxManager.Instance.IsSpeakerMuted);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnToggle);
            VivoxManager.OnCurrentInitializeStateChanged -= VivoxManager_OnCurrentInitializeStateChanged;
        }

        private void OnToggle(bool isOn)
        {
            if (isOn)
                VivoxManager.Instance.UnmuteSpeaker();
            else
                VivoxManager.Instance.MuteSpeaker();
        }

        private void VivoxManager_OnCurrentInitializeStateChanged()
        {
            bool initialized = VivoxManager.CurrentInitializeState == VivoxManager.InitializeState.Initialized;
            toggle.interactable = initialized;
            toggle.SetIsOnWithoutNotify(!VivoxManager.Instance.IsSpeakerMuted);
        }
#endif
    }
}
