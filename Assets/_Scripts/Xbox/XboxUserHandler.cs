using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._Scripts.Xbox
{
    public class XboxUserHandler : MonoBehaviour
    {
        public static XboxUserHandler Instance { get; private set; }
        public delegate void XboxUserLoginStatusChange(bool isLoggedIn);
        public event XboxUserLoginStatusChange OnXboxUserLoginStatusChange;

        public string GamerTag { get; private set; }
        public static string GameConfigTitleId => "60FCC671";
        public static string GameConfigSandbox => "DVKLWP.1";

        private static bool Initialized { get; set; }
        private static string GameConfigScId => "00000000-0000-0000-0000-000060fcc671";
        private static string GameSaveContainerName => "com.TaiwoPictures.Sainthood";


        private bool _isUserLoggedIn;
        private XUserHandle _userHandle;
        private XUserChangeRegistrationToken _registrationToken;
        private XboxSavedDataHandler _savedDataHandler;


        public bool SaveHandlerInitialized { get; private set; }
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
            _savedDataHandler = new XboxSavedDataHandler();

        }

        private void OnDestroy()
        {
            if (_userHandle != null)
            {
                SDK.XUserCloseHandle(_userHandle);
                _userHandle = null;
            }

            if (_registrationToken != null)
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
                SplashSceneController.Instance.FailureReasonPrompt.text = "Failed to sign in user. Please try again.";
            }
        }

        /// <summary>
        /// Begin the process to load data from local or cloud storage.
        /// </summary>
        /// <typeparam name="T">The type of data we are expecting back</typeparam>
        /// <param name="saveFileName">The name of the save file we are loading</param>
        public T LoadData<T>(string saveFileName)
        {
            if (!IsUserLoggedIn || !SaveHandlerInitialized)
            {
                Debug.LogError("User is not logged in or save handler has not been initialized");
                return default;
            }

            var data = _savedDataHandler.LoadAsync(GameSaveContainerName, saveFileName);
            if (data == null || data.Length == 0)
            {
                Debug.LogError("No data found");
                return default;
            }

            var loadedDataAsJson = Encoding.ASCII.GetString(data);
            var loadedData = JsonConvert.DeserializeObject<T>(loadedDataAsJson);
            return loadedData;
        }

        /// <summary>
        /// Begin the process of saving data to local or cloud storage.
        /// </summary>
        /// <typeparam name="T">The type of data we are saving</typeparam>
        /// <param name="fileName">The filename to save the data under</param>
        /// <param name="dataToSave">The object we are saving. Note: Total size of all saved files cannot exceed 12MB</param>
        public T SaveData<T>(string fileName, T dataToSave)
        {
            var dataAsJson = JsonConvert.SerializeObject(dataToSave);
            var dataBytes = Encoding.ASCII.GetBytes(dataAsJson);
            var wasSaveSuccessful = _savedDataHandler.Save(GameSaveContainerName, fileName, dataBytes);

            Debug.Log($"SAVE? {wasSaveSuccessful}");

            return wasSaveSuccessful ? dataToSave : default;
        }

        /// <summary>
        /// Initializes the main Runtime Library.
        /// At the same time we will Creates the Async Dispatch thread which will handle all calls to work.
        /// </summary>
        private static int InitializeGamingRuntime()
        {
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
            // ReSharper disable once IteratorNeverReturns
        }


        private void InitializeAndAddUser()
        {
            Debug.Log($"Starting {nameof(InitializeAndAddUser)}");
            SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
        }

        /// <summary>
        /// Check the result of the AddUserAsync call and if successful, get the user's gamertag
        /// </summary>
        /// <param name="hResult"></param>
        /// <param name="userHandle"></param>
        private void AddUserComplete(int hResult, XUserHandle userHandle)
        {
            if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
                SplashSceneController.Instance.FailureReasonPrompt.text = $"Failed to sign in user. Please try again.\n{HR.NameOf(hResult)}";
                GameManager.Instance.LoadScene("Bootstrapper", LoadSceneMode.Single);
                return;
            }

            _userHandle = userHandle;

            var getGamertagHResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out var gamertag);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(getGamertagHResult))
            {
                Debug.LogError($"FAILED: Could not get user tag, hResult=0x{getGamertagHResult:X} ({HR.NameOf(getGamertagHResult)})");
                return;
            }

            //SDK.XUserFindControllerForUserWithUiAsync(_userHandle, (_, _) =>
            //{
            //    Debug.Log("WE can find controller for user");
            //});

            Debug.Log($"SUCCESS: XUserGetGamertag() returned: '{gamertag}'");
            GamerTag = gamertag;
            IsUserLoggedIn = true;

            //If this works, we can consider loading save data in the main menu
            if (_savedDataHandler.Initialize(_userHandle, GameConfigScId))
            {
                SaveHandlerInitialized = true;
                GameManager.Instance.PlayerLoginSuccess();
            }

        }

        private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
        {
            if (eventType == XUserChangeEvent.SignedOut)
            {
                Debug.Log("User logging out");
                GamerTag = "";
                IsUserLoggedIn = false;

                if (_userHandle != null)
                {
                    SDK.XUserCloseHandle(_userHandle);
                    _userHandle = null;
                }

                InitializeAndAddUser();
            }
        }
    }
}
