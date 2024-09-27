#if UNITY_EDITOR || !UNITY_SERVER
using System.Threading.Tasks;
using Unity.Services.Vivox;
#endif
using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    public class BaseVivoxPositionalEntity : MonoBehaviour
    {
        public bool isReconnect = true;
        public float reconnectDelay = 10f;
        public string channelName;
        public string playerId;
        public string displayName;
        public bool loginOnStart = true;

        public bool IntendedToLogout { get; protected set; } = false;
        public bool IsJoined { get; protected set; } = false;

#if UNITY_EDITOR || !UNITY_SERVER
        protected virtual async void Start()
        {
            await VivoxManager.Instance.InitializeForClient();
            VivoxService.Instance.LoggedIn += Instance_LoggedIn;
            VivoxService.Instance.LoggedOut += Instance_LoggedOut;
            VivoxService.Instance.ChannelJoined += Instance_ChannelJoined;
            VivoxService.Instance.ChannelLeft += Instance_ChannelLeft;
            if (loginOnStart)
                LoginAndForget();
        }

        protected virtual async void OnDestroy()
        {
            VivoxService.Instance.LoggedIn -= Instance_LoggedIn;
            VivoxService.Instance.LoggedOut -= Instance_LoggedOut;
            VivoxService.Instance.ChannelJoined -= Instance_ChannelJoined;
            VivoxService.Instance.ChannelLeft -= Instance_ChannelLeft;
            await Logout();
        }

        protected virtual void Update()
        {
            if (!IsJoined)
                return;
            VivoxService.Instance.Set3DPosition(gameObject, channelName);
        }

        public async void LoginAndForget()
        {
            await Login();
        }

        public async Task Login()
        {
            IntendedToLogout = false;
            await VivoxService.Instance.LoginAsync(new LoginOptions()
            {
                PlayerId = playerId,
                DisplayName = displayName,
            });
        }

        public async Task Logout()
        {
            IntendedToLogout = true;
            IsJoined = false;
            await VivoxService.Instance.LogoutAsync();
        }

        public async Task Join()
        {
            await VivoxService.Instance.JoinPositionalChannelAsync(channelName, ChatCapability.AudioOnly, new Channel3DProperties());
        }

        protected async void Instance_LoggedIn()
        {
            await Join();
        }

        protected async void Instance_LoggedOut()
        {
            while (isReconnect && !IntendedToLogout && !VivoxService.Instance.IsLoggedIn)
            {
                await Task.Delay((int)(reconnectDelay * 1000));
                await Login();
            }
        }

        protected void Instance_ChannelJoined(string channel)
        {
            if (!string.Equals(channel, channelName))
                return;
            IsJoined = true;
        }

        protected void Instance_ChannelLeft(string channel)
        {
            if (!string.Equals(channel, channelName))
                return;
            IsJoined = false;
        }
#endif
    }
}
