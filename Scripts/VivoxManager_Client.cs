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
    public partial class VivoxManager : IVivoxTokenProvider
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
        public static event System.Action OnCurrentInitializeStateChanged = null;
        public static IVivoxTokenProvider TokenProvider { get; set; } = null;
        private static bool _permissionGranted = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _initializeState = InitializeState.None;
            OnCurrentInitializeStateChanged = null;
            TokenProvider = null;
            _permissionGranted = false;
        }

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
                    VivoxService.Instance.SetTokenProvider(this);
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
            if (HasPermissions())
            {
                if (isMicrophoneMuted)
                    MuteMicrophone();
                else
                    UnmuteMicrophone();
            }
            if (isSpeakerMuted)
                MuteSpeaker();
            else
                UnmuteSpeaker();
            int microphoneVolume = PlayerPrefs.GetInt(_prefsKeyMicrophoneVolume, VivoxService.Instance.InputDeviceVolume);
            int speakerVolume = PlayerPrefs.GetInt(_prefsKeySpeakerVolume, VivoxService.Instance.OutputDeviceVolume);
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
            VivoxService.Instance.MuteInputDevice();
            SaveMicrophoneMuteState();
        }

        public void UnmuteMicrophone()
        {
            if (!HasPermissions())
            {
                RequestMicrophonePermissionToUnmute();
            }
            else
            {
                VivoxService.Instance.UnmuteInputDevice();
                SaveMicrophoneMuteState();
            }
        }

        private bool HasPermissions()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return true;
            if (_permissionGranted)
                return true;
#if UNITY_ANDROID
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
#endif
        }

        private void RequestMicrophonePermissionToUnmute()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return;
            if (_permissionGranted)
                return;
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
            if (!string.Equals(permissionName, Permission.Microphone))
                return;
            if (!HasPermissions())
            {
                Debug.LogError($"No microphone permission after requested, Platform: {Application.platform}");
                return;
            }
            _permissionGranted = true;
            VivoxService.Instance.UnmuteInputDevice();
            SaveMicrophoneMuteState();
        }
#else
        private void AsyncOp_completed_Unmute(AsyncOperation asyncOp)
        {
            if (!HasPermissions())
            {
                Debug.LogError($"No microphone permission after requested, Platform: {Application.platform}");
                return;
            }
            _permissionGranted = true;
            VivoxService.Instance.UnmuteInputDevice();
            SaveMicrophoneMuteState();
        }
#endif

        public void MuteSpeaker()
        {
            if (VivoxService.Instance != null)
                VivoxService.Instance.MuteOutputDevice();
            SaveSpeakerMuteState();
        }

        public void UnmuteSpeaker()
        {
            if (VivoxService.Instance != null)
                VivoxService.Instance.UnmuteOutputDevice();
            SaveSpeakerMuteState();
        }

        /// <summary>
        /// Sets the input device volume for the local user. This applies to all active audio sessions. Volume value is clamped between -50 and 50 with a default of 0.
        /// </summary>
        /// <param name="volume"></param>
        public void SetMicrophoneVolume(int volume = 0)
        {
            volume = Mathf.Clamp(volume, -50, 50);
            if (VivoxService.Instance != null)
                VivoxService.Instance.SetInputDeviceVolume(volume);
            SaveMicrophoneVolume();
        }

        /// <summary>
        /// Sets the output device volume for the local user. This applies to all active audio sessions. Volume value is clamped between -50 and 50 with a default of 0.
        /// </summary>
        /// <param name="volume"></param>
        public void SetSpeakerVolume(int volume = 0)
        {
            volume = Mathf.Clamp(volume, -50, 50);
            if (VivoxService.Instance != null)
                VivoxService.Instance.SetOutputDeviceVolume(volume);
            SaveSpeakerVolume();
        }

        public void SaveMicrophoneMuteState()
        {
            PlayerPrefs.SetInt(_prefsKeyMicrophoneMuted, VivoxService.Instance.IsInputDeviceMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SaveSpeakerMuteState()
        {
            PlayerPrefs.SetInt(_prefsKeySpeakerMuted, VivoxService.Instance.IsOutputDeviceMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SaveMicrophoneVolume()
        {
            PlayerPrefs.SetInt(_prefsKeyMicrophoneVolume, VivoxService.Instance.InputDeviceVolume);
            PlayerPrefs.Save();
        }

        public void SaveSpeakerVolume()
        {
            PlayerPrefs.SetInt(_prefsKeySpeakerVolume, VivoxService.Instance.OutputDeviceVolume);
            PlayerPrefs.Save();
        }

        public bool IsMicrophoneMuted => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeyMicrophoneMuted, 1) == 1 : VivoxService.Instance.IsInputDeviceMuted;
        public bool IsSpeakerMuted => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeySpeakerMuted, 0) == 1 : VivoxService.Instance.IsOutputDeviceMuted;
        public int MicrophoneVolume => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeyMicrophoneVolume, 0) : VivoxService.Instance.InputDeviceVolume;
        public int SpeakerVolume => VivoxService.Instance == null ? PlayerPrefs.GetInt(_prefsKeySpeakerVolume, 0) : VivoxService.Instance.OutputDeviceVolume;

        public static async Task LoginAsync(LoginOptions loginOptions)
        {
            if (CurrentInitializeState != InitializeState.Initialized)
                return;
            await VivoxService.Instance.LoginAsync(loginOptions);
        }

        public static async Task LogoutAsync()
        {
            if (CurrentInitializeState != InitializeState.Initialized)
                return;
            await VivoxService.Instance.LogoutAsync();
        }

        public static bool IsLoggedIn => VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn;

        public Task<string> GetTokenAsync(string issuer = null, System.TimeSpan? expiration = null, string targetUserUri = null, string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
        {
            if (TokenProvider == null)
                return null;
            return TokenProvider.GetTokenAsync(issuer, expiration, targetUserUri, action, channelUri, fromUserUri, realm);
        }
    }
}
#endif