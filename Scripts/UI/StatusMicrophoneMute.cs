using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    public class StatusMicrophoneMute : MonoBehaviour
    {
#if !UNITY_SERVER
        public GameObject[] mutedObjects = new GameObject[0];
        public GameObject[] unmutedObjects = new GameObject[0];

        void Update()
        {
            bool isMuted = VivoxManager.Instance != null && VivoxManager.Instance.IsMicrophoneMuted;
            for (int i = 0; i < mutedObjects.Length; ++i)
            {
                if (mutedObjects[i].activeSelf != isMuted)
                    mutedObjects[i].SetActive(isMuted);
            }
            for (int i = 0; i < unmutedObjects.Length; ++i)
            {
                if (unmutedObjects[i].activeSelf != !isMuted)
                    unmutedObjects[i].SetActive(!isMuted);
            }
        }
#endif
    }
}
