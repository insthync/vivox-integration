using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    public class StatusSpeakerMute : MonoBehaviour
    {
#if !UNITY_SERVER
        public GameObject[] mutedObjects = new GameObject[0];
        public GameObject[] unmutedObjects = new GameObject[0];

        void Update()
        {
            bool isMuted = VivoxManager.Instance.IsSpeakerMuted;
            for (int i = 0; i < mutedObjects.Length; ++i)
            {
                mutedObjects[i].SetActive(isMuted);
            }
            for (int i = 0; i < unmutedObjects.Length; ++i)
            {
                unmutedObjects[i].SetActive(!isMuted);
            }
        }
#endif
    }
}
