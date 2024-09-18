using UnityEngine;
using UnityEngine.UI;

namespace Insthync.UnityVivoxIntegration
{
    public class ToggleVivoxMicrophone : MonoBehaviour
    {
        public Toggle toggle;

        private void Awake()
        {
            if (toggle == null)
                toggle = GetComponentInChildren<Toggle>();
            toggle.onValueChanged.AddListener(OnToggle);
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
    }
}
