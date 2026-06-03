using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class SliderVivoxMicrophoneVolume : MonoBehaviour
    {
        public Slider slider;
#if !UNITY_SERVER
        private void Awake()
        {
            if (slider == null)
                slider = GetComponentInChildren<Slider>();
            slider.onValueChanged.AddListener(OnValueChanged);
            slider.minValue = -50;
            slider.maxValue = 50;
            slider.wholeNumbers = true;
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
