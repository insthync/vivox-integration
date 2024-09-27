using System.Threading.Tasks;
using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    public partial class VivoxManager : MonoBehaviour
    {
        [SerializeField]
        private string _server = "";
        [SerializeField]
        private string _domain = "";
        [SerializeField]
        private string _issuer = "";
        [SerializeField]
        private string _key = "";

        // Check to see if we're about to be destroyed.
        static object m_Lock = new object();
        static VivoxManager m_Instance;

        protected bool _isInitializingServer;
        protected bool _isInitializedServer;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static VivoxManager Instance
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        // Search for existing instance.
                        m_Instance = (VivoxManager)FindObjectOfType(typeof(VivoxManager));

                        // Create new instance if one doesn't already exist.
                        if (m_Instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            m_Instance = singletonObject.AddComponent<VivoxManager>();
                            singletonObject.name = typeof(VivoxManager).ToString() + " (Singleton)";
                        }
                    }
                    // Make instance persistent even if its already in the scene
                    DontDestroyOnLoad(m_Instance.gameObject);
                    return m_Instance;
                }
            }
        }

        private void Awake()
        {
            if (m_Instance != this && m_Instance != null)
            {
                Debug.LogWarning(
                    "Multiple VivoxManager detected in the scene. Only one VivoxManager can exist at a time. The duplicate VivoxManager will be destroyed.");
                Destroy(this);
            }
        }

        public async Task InitializeForServer()
        {
            if (_isInitializedServer || _isInitializingServer)
                return;
            _isInitializingServer = true;
            VivoxConfig config = GetComponent<VivoxConfig>();
            if (config != null)
            {
                await config.LoadServer();
                _server = config.Server;
                _domain = config.Domain;
                _issuer = config.Issuer;
                _key = config.Key;
            }
            _isInitializingServer = false;
            _isInitializedServer = true;
        }

        public string GenerateLoginToken(string userId, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateLoginToken(_domain, _issuer, _key, userId, expirationInSeconds);
        }

        public string GenerateJoinToken(string userId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateJoinToken(_domain, _issuer, _key, userId, channelType, channelId, expirationInSeconds);
        }

        public string GenerateJoinToken(string userId, string channelUri, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateJoinToken(_domain, _issuer, _key, userId, channelUri, expirationInSeconds);
        }

        public string GenerateJoinMutedToken(string userId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateJoinMutedToken(_domain, _issuer, _key, userId, channelType, channelId, expirationInSeconds);
        }

        public string GenerateJoinMutedToken(string userId, string channelUri, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateJoinMutedToken(_domain, _issuer, _key, userId, channelUri, expirationInSeconds);
        }

        public string GenerateKickToken(string userId, string kickedUserId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateKickToken(_domain, _issuer, _key, userId, kickedUserId, channelType, channelId, expirationInSeconds);
        }

        public string GenerateMuteToken(string userId, string mutedUserId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateMuteToken(_domain, _issuer, _key, userId, mutedUserId, channelType, channelId, expirationInSeconds);
        }

        public string GenerateVivoxAccessToken(string vxa, uint vxi, string f, string t, string sub, int expirationInSeconds = 90)
        {
            return VivoxTokenGenerator.GenerateVivoxAccessToken(_issuer, _key, vxa, vxi, f, t, sub, expirationInSeconds);
        }

        public void GetChannelTypeAndId(string channelUri, out VivoxChannelType channelType, out string channelId)
        {
            VivoxTokenGenerator.GetChannelTypeAndId(_issuer, channelUri, out channelType, out channelId);
        }
    }
}
