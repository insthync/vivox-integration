#if UNITY_EDITOR || !UNITY_SERVER
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Insthync.UnityVivoxIntegration
{
    public partial class VivoxManager
    {
        public enum InitializeState
        {
            None,
            Initializing,
            Initialized,
        }

        protected InitializeState _initializeState = InitializeState.None;
        public InitializeState CurrentInitializeState => _initializeState;
        public event System.Action OnReadyToSetTokenProvider;

        public async Task InitializeForClient()
        {
#if UNITY_SERVER
            // Do nothing !
            return;
#endif
            // Keep these codes to make it editable
            if (_initializeState != InitializeState.None)
                return;
            _initializeState = InitializeState.Initializing;
            VivoxConfig config = GetComponent<VivoxConfig>();
            if (config != null)
            {
                await config.LoadClient();
                _server = config.Server;
                _domain = config.Domain;
                _issuer = config.Issuer;
            }
            do
            {
                try
                {
                    if (UnityServices.State != ServicesInitializationState.Initialized)
                    {
                        var options = new InitializationOptions();
                        options.SetVivoxCredentials(_server, _domain, _issuer);
                        Debug.Log("Initializing Unity services...");
                        await UnityServices.InitializeAsync(options);
                    }
                    if (VivoxService.Instance == null)
                    {
                        Debug.Log("Not ready to initialize Vivox service yet...");
                        await Task.Delay(1000);
                        continue;
                    }
                    Debug.Log("Initializing Vivox service...");
                    OnReadyToSetTokenProvider?.Invoke();
                    await VivoxService.Instance.InitializeAsync();
                    Debug.Log("Unity and Vivox services are initialized.");
                    break;
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    await Task.Delay(1000);
                }
            } while (!_destroyed);
            _initializeState = InitializeState.Initialized;
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
            if (!HasPermissions())
                RequestMicrophonePermissionToUnmute();
            else
                VivoxService.Instance.UnmuteInputDevice();
        }

        private bool HasPermissions()
        {
#if UNITY_ANDROID
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
#endif
        }

        private void RequestMicrophonePermissionToUnmute()
        {
#if UNITY_ANDROID
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += Callbacks_PermissionGranted;
            Permission.RequestUserPermission(Permission.Microphone, callbacks);
#else
            AsyncOperation asyncOp = Application.RequestUserAuthorization(UserAuthorization.Microphone);
            asyncOp.completed += AsyncOp_completed_Unmute;
#endif
        }

#if UNITY_ANDROID
        private void Callbacks_PermissionGranted(string permissionName)
        {
            UnmuteMicrophone();
        }
#else
        private void AsyncOp_completed_Unmute(AsyncOperation asyncOp)
        {
            UnmuteMicrophone();
        }
#endif

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

        public bool IsMicrophoneMuted => VivoxService.Instance == null ? true : VivoxService.Instance.IsInputDeviceMuted;
        public bool IsSpeakerMuted => VivoxService.Instance == null ? true : VivoxService.Instance.IsOutputDeviceMuted;
        public int MicrophoneVolume => VivoxService.Instance == null ? 0 : VivoxService.Instance.InputDeviceVolume;
        public int SpeakerVolume => VivoxService.Instance == null ? 0 : VivoxService.Instance.OutputDeviceVolume;
    }
}
#endif