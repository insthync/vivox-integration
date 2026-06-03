using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class ToggleVivoxMicrophone : MonoBehaviour
    {
        public Toggle toggle;
#if !UNITY_SERVER
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
            toggle.SetIsOnWithoutNotify(!VivoxManager.Instance.IsMicrophoneMuted);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnToggle);
        }

        private void OnToggle(bool isOn)
        {
            if (isOn)
                VivoxManager.Instance.UnmuteMicrophone();
            else
                VivoxManager.Instance.MuteMicrophone();
        }

        private void VivoxManager_OnCurrentInitializeStateChanged()
        {
            bool initialized = VivoxManager.CurrentInitializeState == VivoxManager.InitializeState.Initialized;
            toggle.interactable = initialized;
            toggle.SetIsOnWithoutNotify(!VivoxManager.Instance.IsMicrophoneMuted);
        }
#endif
    }
}
