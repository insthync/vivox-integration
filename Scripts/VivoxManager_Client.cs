#if !UNITY_SERVER
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Vivox;

namespace Insthync.UnityVivoxIntegration
{
    public partial class VivoxManager : IVivoxTokenProvider
    {
        private async void InitializeForClient()
        {
            var options = new InitializationOptions();
            if (CheckManualCredentials())
            {
                options.SetVivoxCredentials(_server, _domain, _issuer, _key);
            }
            VivoxService.Instance.SetTokenProvider(this);
            await UnityServices.InitializeAsync(options);
            await VivoxService.Instance.InitializeAsync();
        }

        private bool CheckManualCredentials()
        {
            return !(string.IsNullOrWhiteSpace(_issuer) && string.IsNullOrWhiteSpace(_domain) && string.IsNullOrWhiteSpace(_server));
        }

        public Task<string> GetTokenAsync(string issuer = null, TimeSpan? expiration = null, string targetUserUri = null, string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
        {
            throw new NotImplementedException();
        }

        public void ToggleMicrophone()
        {
            if (VivoxService.Instance.IsInputDeviceMuted)
                VivoxService.Instance.UnmuteInputDevice();
            else
                VivoxService.Instance.MuteInputDevice();
        }

        public void ToggleSpeaker()
        {
            if (VivoxService.Instance.IsOutputDeviceMuted)
                VivoxService.Instance.UnmuteOutputDevice();
            else
                VivoxService.Instance.MuteOutputDevice();
        }

        public void MuteMicrophone()
        {
            VivoxService.Instance.MuteInputDevice();
        }

        public void UnmuteMicrophone()
        {
            VivoxService.Instance.UnmuteInputDevice();
        }

        public void MuteSpeaker()
        {
            VivoxService.Instance.MuteOutputDevice();
        }

        public void UnmuteSpeaker()
        {
            VivoxService.Instance.UnmuteOutputDevice();
        }

        /// <summary>
        /// Sets the input device volume for the local user. This applies to all active audio sessions. Volume value is clamped between -50 and 50 with a default of 0.
        /// </summary>
        /// <param name="volume"></param>
        public void SetMicrophoneVolume(int volume = 0)
        {
            VivoxService.Instance.SetInputDeviceVolume(volume);
        }

        /// <summary>
        /// Sets the output device volume for the local user. This applies to all active audio sessions. Volume value is clamped between -50 and 50 with a default of 0.
        /// </summary>
        /// <param name="volume"></param>
        public void SetSpeakerVolume(int volume = 0)
        {
            VivoxService.Instance.SetOutputDeviceVolume(volume);
        }

        public bool IsMicrophoneMuted => VivoxService.Instance.IsInputDeviceMuted;
        public bool IsSpeakerMuted => VivoxService.Instance.IsOutputDeviceMuted;
        public int MicrophoneVolume => VivoxService.Instance.InputDeviceVolume;
        public int SpeakerVolume => VivoxService.Instance.OutputDeviceVolume;
    }
}
#endif