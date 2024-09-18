using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    public abstract class BaseVivoxPositionalEntity : MonoBehaviour
    {
        public bool isReconnect = true;
        public float reconnectDelay = 10f;
        public string channelName;
        public string playerId;
        public string displayName;
        public bool loginOnStart = true;

        private bool _intendedToLogout = false;
        private bool _isJoined = false;

        protected virtual void Start()
        {
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

        private void Update()
        {
            if (!_isJoined)
                return;
            VivoxService.Instance.Set3DPosition(gameObject, channelName);
        }

        public async void LoginAndForget()
        {
            await Login();
        }

        public async Task Login()
        {
            _intendedToLogout = false;
            await VivoxService.Instance.LoginAsync(new LoginOptions()
            {
                PlayerId = playerId,
                DisplayName = displayName,
            });
        }

        public async Task Logout()
        {
            _intendedToLogout = true;
            _isJoined = false;
            await VivoxService.Instance.LogoutAsync();
        }

        public async Task Join()
        {
            await VivoxService.Instance.JoinPositionalChannelAsync(channelName, ChatCapability.AudioOnly, new Channel3DProperties());
        }

        private async void Instance_LoggedIn()
        {
            await Join();
        }

        private async void Instance_LoggedOut()
        {
            while (isReconnect && !_intendedToLogout && !VivoxService.Instance.IsLoggedIn)
            {
                await Task.Delay((int)(reconnectDelay * 1000));
                await Logout();
            }
        }

        private void Instance_ChannelJoined(string channel)
        {
            if (!string.Equals(channel, channelName))
                return;
            _isJoined = true;
        }

        private void Instance_ChannelLeft(string channel)
        {
            if (!string.Equals(channel, channelName))
                return;
            _isJoined = false;
        }
    }
}
