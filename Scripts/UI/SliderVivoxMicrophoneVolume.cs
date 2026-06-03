using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class SliderVivoxMicrophoneVolume : MonoBehaviour
    {
#if !UNITY_SERVER
        public Slider slider;

        private void Awake()
        {
            if (slider == null)
                slider = GetComponentInChildren<Slider>();
            slider.onValueChanged.AddListener(OnValueChanged);
            VivoxManager_OnCurrentInitializeStateChanged();
            VivoxManager.OnCurrentInitializeStateChanged += VivoxManager_OnCurrentInitializeStateChanged;
        }

        private void OnEnable()
        {
            slider.SetValueWithoutNotify(VivoxManager.Instance.MicrophoneVolume);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
            VivoxManager.OnCurrentInitializeStateChanged -= VivoxManager_OnCurrentInitializeStateChanged;
        }

        private void OnValueChanged(float value)
        {
            VivoxManager.Instance.SetMicrophoneVolume(Mathf.CeilToInt(value));
        }

        private void VivoxManager_OnCurrentInitializeStateChanged()
        {
            bool initialized = VivoxManager.CurrentInitializeState == VivoxManager.InitializeState.Initialized;
            slider.interactable = initialized;
            slider.SetValueWithoutNotify(VivoxManager.Instance.MicrophoneVolume);
        }
#endif
    }
}
