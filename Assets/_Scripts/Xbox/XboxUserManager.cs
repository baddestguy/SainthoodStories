using System;
using System.Collections;
using Unity.XGamingRuntime;
using UnityEngine;

namespace Assets._Scripts.Xbox
{
    public class XboxUserManager : MonoBehaviour
    {
        public static XboxUserManager Instance { get; private set; }
        public delegate void XboxUserLoginStatusChange(bool isLoggedIn);
        public event XboxUserLoginStatusChange OnXboxUserLoginStatusChange;

        public string GamerTag { get; private set; }

        private static bool Initialized { get; set; }
        private static string GameConfigScId => "00000000-0000-0000-0000-000060fcc671";
        public static string GameConfigTitleId => "60FCC671";
        public static string GameConfigSandbox => "DVKLWP.1";

        //private static GameObject DispatchGDKGameObject;
        private bool _isUserLoggedIn;
        private XUserHandle _userHandle;
        private XUserChangeRegistrationToken _registrationToken;

        public bool IsUserLoggedIn

        {
            get => _isUserLoggedIn;
            set
            {
                _isUserLoggedIn = value;
                OnXboxUserLoginStatusChange?.Invoke(_isUserLoggedIn);
            }
        }

        void Start()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (_userHandle != null)
            {
                SDK.XUserCloseHandle(_userHandle);
                _userHandle = null;
            }

            if(_registrationToken != null)
            {
                SDK.XUserUnregisterForChangeEvent(_registrationToken);
            }
        }

        public void TryLogInUser()
        {
            try
            {
                if (!Initialized)
                {
                    Debug.Log("GDK XGameRuntime Library not initialized.");

                    Instance = this;

                    if (Unity.XGamingRuntime.Interop.HR.FAILED(InitializeGamingRuntime()) || !InitializeXboxLive(GameConfigScId))
                    {
                        Initialized = false;
                        return;
                    }

                    // Might remove later. Confirm that the console believes the same things I do
                    int hResult = SDK.XGameGetXboxTitleId(out var titleId);
                    if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
                    {
                        Debug.LogError($"FAILED: Could not get TitleID! hResult: 0x{hResult:x} ({HR.NameOf(hResult)})");
                    }

                    if (titleId.ToString("X").ToLower().Equals(GameConfigTitleId.ToLower()) == false)
                    {
                        Debug.LogError($"WARNING! Expected Title Id: {GameConfigTitleId} got: {titleId:X}");
                    }

                    hResult = SDK.XSystemGetXboxLiveSandboxId(out var sandboxId);
                    if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
                    {
                        Debug.LogError($"FAILED: Could not get SandboxID! HResult: 0x{hResult:x} ({HR.NameOf(hResult)})");
                    }

                    if (sandboxId.Equals(GameConfigSandbox) == false)
                    {
                        Debug.LogError($"WARNING! Expected sandbox Id: {GameConfigSandbox} got: {sandboxId}");
                    }

                    Debug.Log($"GDK XGameRuntime Library initialized: {Initialized}");
                    Debug.Log($"GDK Xbox Live API SCID: {GameConfigScId}");
                    Debug.Log($"GDK TitleId: {GameConfigTitleId}");
                    Debug.Log($"GDK Sandbox: {GameConfigSandbox}");

                    InitializeAndAddUser();
                    SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _registrationToken);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to log in user: {e.Message}");
            }
        }

        /// <summary>
        /// Initializes the main Runtime Library.
        /// At the same time we will Creates the Async Dispatch thread which will handle all calls to work.
        /// </summary>
        private static int InitializeGamingRuntime()
        {
            // We do not want stack traces for all log statements. (Exceptions logged
            // with Debug.LogException will still have stack traces though.):
            //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            Debug.Log("Initializing XGame Runtime Library.");

            if (Initialized)
            {
                Debug.Log("Gaming Runtime already initialized.");
                return 0;
            }

            var hResult = SDK.XGameRuntimeInitialize();
            if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Initialize XGameRuntime, HResult: 0x{hResult:X} ({HR.NameOf(hResult)})");
                return hResult;
            }

            StartAsyncDispatchCoroutine();

            return 0;
        }

        /// <summary>
        /// Initializes the Xbox Live Basic API this is required for all Xbox Live API calls.
        /// </summary>
        /// <returns>The HResult value of initializing Xbox Live</returns>
        private static bool InitializeXboxLive(string scid)
        {
            Debug.Log($"Initializing Xbox Live API (SCID: {scid}).");

            int hResult = SDK.XBL.XblInitialize(scid);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult) && hResult != HR.E_XBL_ALREADY_INITIALIZED)
            {
                Debug.LogError($"FAILED: Initialize Xbox Live, HResult: 0x{hResult:X}, {HR.NameOf(hResult)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// This allows the native code space to create asynchronous blocks and work in parallel to common calls.
        /// </summary>
        private static void StartAsyncDispatchCoroutine()
        {
            // We need to execute SDK.XTaskQueueDispatch(0) to pump all GDK events.
            //if (DispatchGDKGameObject == null)
            {
                int hResult = SDK.CreateDefaultTaskQueue();
                if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
                {
                    Debug.LogError($"FAILED: XTaskQueueCreate, HResult: 0x{hResult:X}");
                    return;
                }

                Instance.StartCoroutine(DispatchGDKTaskQueue());
            }
        }

        /// <summary>
        /// The actual Dispatch Task Queue - This executes all GDK Asynchronous block work natively
        /// </summary>
        private static IEnumerator DispatchGDKTaskQueue()
        {
            while (true)
            {
                SDK.XTaskQueueDispatch(0);
                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns.
        }


        private void InitializeAndAddUser()
        {
            Debug.Log($"Starting {nameof(InitializeAndAddUser)}");
            SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
        }

        private void AddUserComplete(int hResult, XUserHandle userHandle)
        {
            if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            _userHandle = userHandle;

            CompletePostSignInInitialization();
        }

        private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
        {
            if (eventType == XUserChangeEvent.SignedOut)
            {
                Debug.Log("User logging out");
                GamerTag = "";

                if (_userHandle != null)
                {
                    SDK.XUserCloseHandle(_userHandle);
                    _userHandle = null;
                }

                InitializeAndAddUser();
            }
        }

        private void CompletePostSignInInitialization()
        {
            int hResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out var gamertag);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Could not get user tag, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            SDK.XUserFindControllerForUserWithUiAsync(_userHandle, (hresult, id) =>
            {
                Debug.Log("WE can find controller for user");
            });

            Debug.Log($"SUCCESS: XUserGetGamertag() returned: '{gamertag}'");
            GamerTag = gamertag;
            GameManager.Instance.PlayerLoginSuccess();
        }
    };

}
