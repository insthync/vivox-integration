#if UNITY_EDITOR || !UNITY_SERVER
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;

namespace Insthync.UnityVivoxIntegration
{
    public partial class VivoxManager
    {
        public const string PERMISSION_RECORD_AUDIO = "android.permission.RECORD_AUDIO";
        protected bool _isInitializingClient;
        protected bool _isInitializedClient;

        public async Task InitializeForClient()
        {
            if (_isInitializedClient || _isInitializingClient)
                return;
            _isInitializingClient = true;
            VivoxConfig config = GetComponent<VivoxConfig>();
            if (config != null)
            {
                await config.LoadClient();
                _server = config.Server;
                _domain = config.Domain;
                _issuer = config.Issuer;
            }
            var options = new InitializationOptions();
            options.SetVivoxCredentials(_server, _domain, _issuer);
            do
            {
                try
                {
                    await UnityServices.InitializeAsync(options);
                    await VivoxService.Instance.InitializeAsync();
                    break;
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    await Task.Delay(1000);
                }
            } while (true);
            _isInitializingClient = false;
            _isInitializedClient = true;
        }

        public void ToggleMicrophone()
        {
            if (VivoxService.Instance.IsInputDeviceMuted)
                UnmuteMicrophone();
            else
                MuteMicrophone();
        }

        public void ToggleSpeaker()
        {
            if (VivoxService.Instance.IsOutputDeviceMuted)
                UnmuteSpeaker();
            else
                MuteSpeaker();
        }

        public void MuteMicrophone()
        {
            VivoxService.Instance.MuteInputDevice();
        }

        public void UnmuteMicrophone()
        {
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                RequestMicrophonePermissionToUnmute();
                return;
            }
            VivoxService.Instance.UnmuteInputDevice();
        }

        private void RequestMicrophonePermissionToUnmute()
        {
            AsyncOperation asyncOp = Application.RequestUserAuthorization(UserAuthorization.Microphone);
            asyncOp.completed += AsyncOp_completed_Unmute;
        }

        private void AsyncOp_completed_Unmute(AsyncOperation asyncOp)
        {
            asyncOp.completed -= AsyncOp_completed_Unmute;
            UnmuteMicrophone();
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