using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
using HR = Unity.XGamingRuntime.HR;

namespace Assets._Scripts.Xbox
{
    public class XboxUserHandler : MonoBehaviour
    {
        public static XboxUserHandler Instance { get; private set; }
        public delegate void XboxUserLoginStatusChange(bool isLoggedIn, string message = "", bool isError = false);
        public event XboxUserLoginStatusChange OnXboxUserLoginStatusChange;
        public XboxSavedDataHandler SavedDataHandler;

        public ulong UserId { get; private set; }
        public string GamerTag { get; private set; }
        public ulong LocalUserId { get; private set; }
        public static string GameConfigTitleId => "60FCC671";
        public static string GameConfigSandbox => "DVKLWP.1";

        private static bool Initialized { get; set; }
        private static string GameConfigScId => "00000000-0000-0000-0000-000060fcc671";
        private static string GameSaveContainerName => "com.TaiwoPictures.Sainthood";


        private bool _isUserLoggedIn;
        private XUserHandle _userHandle;
        private XUserChangeRegistrationToken _registrationToken;
        private XblContextHandle _xblContextHandle;

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
            SavedDataHandler = new XboxSavedDataHandler();

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
                OnXboxUserLoginStatusChange?.Invoke(false, "Logging In...");

                if (!Initialized)
                {
                    Instance = this;

                    if (Unity.XGamingRuntime.Interop.HR.FAILED(InitializeGamingRuntime()) || !InitializeXboxLive(GameConfigScId))
                    {
                        Initialized = false;
                        OnXboxUserLoginStatusChange?.Invoke(false, "Failed to initialize XGame Runtime", true);
                        return;
                    }

                    Initialized = true;
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
                OnXboxUserLoginStatusChange?.Invoke(false, $"Failed to sign in user. Please try again. \n {e.Message}", true);
            }
        }

        /// <summary>
        /// Begin the process to load data from local or cloud storage.
        /// </summary>
        /// <typeparam name="T">The type of data we are expecting back</typeparam>
        /// <param name="saveFileName">The name of the save file we are loading</param>
        /// <param name="killAsyncSaves">Kill any async saves that may be happening</param>
        public T LoadData<T>(string saveFileName, bool killAsyncSaves = true)
        {
            if (!IsUserLoggedIn || !SaveHandlerInitialized)
            {
                Debug.LogError("User is not logged in or save handler has not been initialized");
                return default;
            }

            var data = SavedDataHandler.Load(GameSaveContainerName, saveFileName, killAsyncSaves);
            if (data == null || data.Length == 0)
            {
                Debug.LogError($"No data found for {saveFileName}");
                return default;
            }

            var loadedDataAsJson = Encoding.ASCII.GetString(data);

            Debug.Log($"Returning {loadedDataAsJson}");
            var loadedData = JsonConvert.DeserializeObject<T>(loadedDataAsJson);
            return loadedData;
        }

        public bool Save<T>(string saveFileName, T data)
        {
            var dataAsJson = JsonConvert.SerializeObject(data);
            var dataBytes = Encoding.ASCII.GetBytes(dataAsJson);
            var saveSuccess = SavedDataHandler.Save(GameSaveContainerName, saveFileName, dataBytes);
            return saveSuccess;
        }

        public void QueueSave<T>(string filename, T data)
        {
            try
            {
                //Add the save to the queue
                Debug.Log($"Added {filename} to the save queue");
                SavedDataHandler.SaveQueue.Enqueue((filename, data));

                //If the save queue is not already running, start it
                if (!SavedDataHandler.IsProcessingAsyncSave)
                {
                    SavedDataHandler.IsProcessingAsyncSave = true;
                    StartCoroutine(ProcessSaveQueue());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to queue save: {e.Message}");
            }
        }

        private IEnumerator ProcessSaveQueue()
        {
            Debug.Log("Starting save queue processing");
            // process queue in order until it is empty
            while (SavedDataHandler.SaveQueue.Count > 0)
            {
                Debug.Log($"Current queue count is {SavedDataHandler.SaveQueue.Count}");
                var dequeueSuccess = SavedDataHandler.SaveQueue.TryDequeue(out var saveDetails);

                if (dequeueSuccess)
                {
                    Debug.Log($"Processing next save file");
                    yield return StartCoroutine(SavedDataHandler.SaveAsync(GameSaveContainerName, saveDetails.Filename, saveDetails.SaveData));
                }
                else
                {
                    Debug.LogError("Failed to dequeue save file");
                }
            }

            SavedDataHandler.IsProcessingAsyncSave = false;
            
            // Eltee: I can probably remove it but things work, and I'm not in the mood to start debugging why it would stop working if removing the below statement breaks it.
            // ReSharper disable once RedundantJumpStatement
            yield break;
        }

        /// <summary>
        /// Unlock an achievement for the current logged in xbox user.
        /// </summary>
        /// <param name="achievementId">The achievement ID as configured in Partner Center</param>
        /// <param name="progressLevel">The value we want to increase the achievement level to. (0 - 100)</param>
        public void UnlockAchievement(string achievementId, int progressLevel = 100)
        {
            if (!GameSettings.Instance.IsXboxMode) return;

            // This API will work even when offline.  Offline updates will be posted by the system when connection is
            // re-established even if the title isn’t running. If the achievement has already been unlocked or the progress
            // value is less than or equal to what is currently recorded on the server HTTP_E_STATUS_NOT_MODIFIED (0x80190130L)
            // will be returned.
            SDK.XBL.XblAchievementsUpdateAchievementAsync(
                _xblContextHandle,
                UserId,
                achievementId,
                (uint)progressLevel,
                hResult =>
                {
                    if (hResult == HR.HTTP_E_STATUS_NOT_MODIFIED)
                    {
                        Debug.Log($"Achievement {achievementId} ALREADY Unlocked!");
                        return;
                    }

                    if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
                    {
                        Debug.LogError($"FAILED: Achievement {achievementId} Update, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                        return;
                    }

                    Debug.Log($"SUCCESS: {achievementId} has been updated to level {progressLevel}");
                }
            );
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
            SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
        }

        /// <summary>
        /// Check the result of the AddUserAsync call and if successful, get the user's gamertag
        /// </summary>
        /// <param name="hResult"></param>
        /// <param name="userHandle"></param>
        private void AddUserComplete(int hResult, XUserHandle userHandle)
        {
            OnXboxUserLoginStatusChange?.Invoke(false, "Completing Add User...");

            if (Unity.XGamingRuntime.Interop.HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");

                OnXboxUserLoginStatusChange?.Invoke(false, $"Failed to sign in user. Please try again.\n{HR.NameOf(hResult)}", true);
                GameManager.Instance.LoadScene("Bootloader", LoadSceneMode.Single);
                return;
            }

            _userHandle = userHandle;

            var getGamerTagResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out var gamertag);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(getGamerTagResult))
            {
                Debug.LogError($"FAILED: Could not get user tag, hResult=0x{getGamerTagResult:X} ({HR.NameOf(getGamerTagResult)})");
                OnXboxUserLoginStatusChange?.Invoke(false, $"FAILED: Could not get user tag, hResult=0x{getGamerTagResult:X} ({HR.NameOf(getGamerTagResult)})", true);
                return;
            }

            var userIdHResult = SDK.XUserGetId(_userHandle, out var userId);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(userIdHResult))
            {
                Debug.LogError($"FAILED: Could not get user ID, hResult=0x{userIdHResult:X} ({HR.NameOf(userIdHResult)})");
                OnXboxUserLoginStatusChange?.Invoke(false, $"FAILED: Could not get user ID, hResult=0x{userIdHResult:X} ({HR.NameOf(userIdHResult)})", true);
                return;
            }

            //SDK.XUserFindControllerForUserWithUiAsync(_userHandle, (_, _) =>
            //{
            //    Debug.Log("WE can find controller for user");
            //});
            var localIdResult =  SDK.XUserGetLocalId(_userHandle, out var userLocalId);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(localIdResult))
            {
                Debug.LogError($"FAILED: Could not get user ID, hResult=0x{localIdResult:X} ({HR.NameOf(localIdResult)})");
                OnXboxUserLoginStatusChange?.Invoke(false, $"FAILED: Could not get user local ID, hResult=0x{localIdResult:X} ({HR.NameOf(localIdResult)})", true);
                return;
            }

            Debug.Log($"SUCCESS: XUserGetGamertag() returned: '{gamertag}' for UserId: '{userId}' and LocalId: '{userLocalId.Value}'");
            UserId = userId;
            LocalUserId = userLocalId.Value;
            GamerTag = gamertag;
            IsUserLoggedIn = true;

            var xblContextResult = SDK.XBL.XblContextCreateHandle(_userHandle, out _xblContextHandle);
            if (Unity.XGamingRuntime.Interop.HR.FAILED(xblContextResult))
            {
                Debug.LogError($"FAILED: Could not create context handle, hResult=0x{xblContextResult:X} ({HR.NameOf(xblContextResult)})");
                OnXboxUserLoginStatusChange?.Invoke(false, $"FAILED: Could not create context handle, hResult=0x{xblContextResult:X} ({HR.NameOf(xblContextResult)})", true);
                return;
            }

            if (SavedDataHandler.Initialize(_userHandle, GameConfigScId))
            {
                SaveHandlerInitialized = true;
                OnXboxUserLoginStatusChange?.Invoke(false, "Loading Game Settings...");
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
