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

        protected static InitializeState _initializeState = InitializeState.None;
        public static InitializeState CurrentInitializeState
        {
            get => _initializeState;
            set
            {
                if (_initializeState != value)
                {
                    _initializeState = value;
                    OnCurrentInitializeStateChanged?.Invoke();
                }
            }
        }
        public static event System.Action OnCurrentInitializeStateChanged;
        public static event System.Action OnReadyToSetTokenProvider;

        public async Task InitializeForClient()
        {
#if UNITY_SERVER
            // Do nothing !
            return;
#endif
            // Keep these codes to make it editable
            if (CurrentInitializeState != InitializeState.None)
                return;
            CurrentInitializeState = InitializeState.Initializing;
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
            bool isMicrophoneMuted = PlayerPrefs.GetInt(_prefsKeyMicrophoneMuted, VivoxService.Instance.IsInputDeviceMuted ? 1 : 0) == 1;
            bool isSpeakerMuted = PlayerPrefs.GetInt(_prefsKeySpeakerMuted, VivoxService.Instance.IsOutputDeviceMuted ? 1 : 0) == 1;
            if (isMicrophoneMuted)
                MuteMicrophone();
            else
                UnmuteMicrophone();
            if (isSpeakerMuted)
                MuteSpeaker();
            else
                UnmuteSpeaker();
            int microphoneVolume = PlayerPrefs.GetInt(_prefsKeyMicrophoneVolume, 0);
            int speakerVolume = PlayerPrefs.GetInt(_prefsKeySpeakerVolume, 0);
            SetMicrophoneVolume(microphoneVolume);
            SetSpeakerVolume(speakerVolume);
            CurrentInitializeState = InitializeState.Initialized;
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
            PlayerPrefs.SetInt(_prefsKeyMicrophoneMuted, 1);
            PlayerPrefs.Save();
            VivoxService.Instance.MuteInputDevice();
        }

        public void UnmuteMicrophone()
        {
            PlayerPrefs.SetInt(_prefsKeyMicrophoneMuted, 0);
            PlayerPrefs.Save();
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
            PlayerPrefs.SetInt(_prefsKeySpeakerMuted, 1);
            PlayerPrefs.Save();
            VivoxService.Instance.MuteOutputDevice();
        }

        public void UnmuteSpeaker()
        {
            PlayerPrefs.SetInt(_prefsKeySpeakerMuted, 0);
            PlayerPrefs.Save();
            VivoxService.Instance.UnmuteOutputDevice();
        }

        /// <summary>
        /// Sets the input device volume for the local user. This applies to all active audio sessions. Volume value is clamped between -50 and 50 with a default of 0.
        /// </summary>
        /// <param name="volume"></param>
        public void SetMicrophoneVolume(int volume = 0)
        {
            PlayerPrefs.SetInt(_prefsKeyMicrophoneVolume, volume);
            PlayerPrefs.Save();
            VivoxService.Instance.SetInputDeviceVolume(volume);
        }

        /// <summary>
        /// Sets the output device volume for the local user. This applies to all active audio sessions. Volume value is clamped between -50 and 50 with a default of 0.
        /// </summary>
        /// <param name="volume"></param>
        public void SetSpeakerVolume(int volume = 0)
        {
            PlayerPrefs.SetInt(_prefsKeySpeakerVolume, volume);
            PlayerPrefs.Save();
            VivoxService.Instance.SetOutputDeviceVolume(volume);
        }

        public bool IsMicrophoneMuted => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeyMicrophoneMuted, 0) == 1 : VivoxService.Instance.IsInputDeviceMuted;
        public bool IsSpeakerMuted => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeySpeakerMuted, 0) == 1 : VivoxService.Instance.IsOutputDeviceMuted;
        public int MicrophoneVolume => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeyMicrophoneVolume, 0) : VivoxService.Instance.InputDeviceVolume;
        public int SpeakerVolume => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeySpeakerVolume, 0) : VivoxService.Instance.OutputDeviceVolume;
    }
}
#endif