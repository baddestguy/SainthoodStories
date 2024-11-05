using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.XGamingRuntime;
using UnityEngine;
using HR = Unity.XGamingRuntime.Interop.HR;

namespace Assets._Scripts.Xbox
{
    public class XboxSavedDataHandler
    {
        public bool IsProcessingAsyncSave { get; set; }
        public ConcurrentQueue<(string Filename, object SaveData)> SaveQueue { get; private set; } = new();

        private XUserHandle _userHandle;
        private XGameSaveProviderHandle _gameSaveProviderHandle;
        private bool _shouKillAsyncSaves;


        ~XboxSavedDataHandler()
        {
            SDK.XGameSaveCloseProvider(_gameSaveProviderHandle);
            SDK.XUserCloseHandle(_userHandle);
        }

        /// <summary>
        /// Intializes the save game wrapper and may initiate a sync of all of the containers for specified user of the game.
        /// </summary>
        /// <param name="userHandle">Handle of the Xbox Live User whose saves are to be managed.</param>
        /// <param name="scid">Service configuration ID (SCID) of the game whose saves are to be managed.</param>
        public bool Initialize(XUserHandle userHandle, string scid)
        {
            _userHandle = null;
            _gameSaveProviderHandle = null;
            SaveQueue = new ConcurrentQueue<(string, object)>();

            var hr = SDK.XUserDuplicateHandle(userHandle, out _userHandle);
            if (HR.FAILED(hr))
            {
                return false;
            }

            var hResult = SDK.XGameSaveInitializeProvider(_userHandle, scid, false, out _gameSaveProviderHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Initialize game save provider. ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the data from a given blob (file) that is within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to load data from.</param>
        /// <param name="shouldKillAsyncSaves">Avoid scenario where blob info is invalidated due to async save after load process started</param>
        /// <param name="retryCount"></param>
        public byte[] Load(string containerName, string blobName, bool shouldKillAsyncSaves = true, int retryCount = 3)
        {
            while (retryCount > 0)
            {
                //If we are currently saving any async files, cancel it. Next time don't quit the game while saving.
                if (shouldKillAsyncSaves && IsProcessingAsyncSave)
                {
                    Debug.Log($"Kill {SaveQueue.Count} async saves because we are attempting to load data");
                    _shouKillAsyncSaves = true;
                    SaveQueue.Clear();
                }

                var loadedData = LoadImplementation(containerName, blobName);
                if (loadedData != default)
                {
                    return loadedData;
                }
                retryCount--;
            }

            return default;

            byte[] LoadImplementation(string saveContainerName, string saveBlobName)
            {
                try
                {
                    Debug.Log($"Loading save for container {saveContainerName} and blob {saveBlobName}");
                    //Step 1: Create a container
                    if (!TryCreateSaveContainer(saveContainerName, out var gameSaveContainerHandle))
                    {
                        return default;
                    }

                    //Step 2: Get metadata about the blob
                    var blobInfoResult =
                        SDK.XGameSaveEnumerateBlobInfoByName(gameSaveContainerHandle, saveBlobName, out var blobInfos);

                    if (HR.FAILED(blobInfoResult))
                    {
                        Debug.LogError(
                            $"FAILED: Enumerate blob info by name. HResult: ({Unity.XGamingRuntime.HR.NameOf(blobInfoResult)}  0x{blobInfoResult:x})");
                        return default;
                    }

                    //Step 3: Read the blob 
                    var readBlobResult = SDK.XGameSaveReadBlobData(gameSaveContainerHandle,
                        blobInfos,
                        out var data
                    );

                    if (HR.FAILED(readBlobResult))
                    {
                        if (readBlobResult == Unity.XGamingRuntime.HR.E_GS_USER_NOT_REGISTERED_IN_SERVICE)
                        {
                            Debug.LogError(
                                $"FAILED: User may be logging out x{readBlobResult:X} ({Unity.XGamingRuntime.HR.NameOf(readBlobResult)})");
                        }
                        else
                        {
                            Debug.LogError(
                                $"FAILED: Game load, hResult=0x{readBlobResult:X} ({Unity.XGamingRuntime.HR.NameOf(readBlobResult)})");
                        }

                        return default;
                    }

                    return data.SingleOrDefault(x => x.Info.Name.Equals(saveBlobName))?.Data;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception while loading save: {e}");
                    return default;
                }
            }
        }

        /// <summary>
        /// Saves data to the blob (file) within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to save the data to within the specified container.</param>
        /// <param name="blobData">The bytes that are to be written to the blob (file).</param>
        public bool Save(string containerName, string blobName, byte[] blobData)
        {
            if(IsProcessingAsyncSave)
            {
                Debug.Log($"Kill {SaveQueue.Count} async saves because we are doing a synchronous save");
                _shouKillAsyncSaves = true;
                SaveQueue.Clear();
            }

            //Step 1: Create a container
            if (!TryCreateSaveContainer(containerName, out var gameSaveContainerHandle))
            {
                return false;
            }

            //Step 2:  Start container Update
            var containerUpdateResult = SDK.XGameSaveCreateUpdate(gameSaveContainerHandle, blobName, out var gameSaveContainerUpdateHandle);
            if (HR.FAILED(containerUpdateResult))
            {
                Debug.LogError($"Error when creating the {blobName} container display  update process. HResult: ({Unity.XGamingRuntime.HR.NameOf(containerUpdateResult)}  0x{containerUpdateResult:x})");
                return false;
            }

            //Step 3: Submit data blob to write
            var submitResult = SDK.XGameSaveSubmitBlobWrite(gameSaveContainerUpdateHandle, blobName, blobData);
            if (HR.FAILED(submitResult))
            {
                Debug.LogError($"Error when submitting the blob {blobName}. HResult: ({Unity.XGamingRuntime.HR.NameOf(submitResult)}  0x{submitResult:x})");
                return false;
            }

            //Submit blob update
            var hResult = SDK.XGameSaveSubmitUpdate(gameSaveContainerUpdateHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"Error when submitting container updated process. HResult: ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                return false;
            }

            SDK.XGameSaveCloseUpdate(gameSaveContainerUpdateHandle);
            SDK.XGameSaveCloseContainer(gameSaveContainerHandle);

            return true;
        }

        /// <summary>
        /// Begin the process of saving data to local or cloud storage.
        /// </summary>
        /// <typeparam name="T">The type of data we are saving</typeparam>
        /// <param name="containerName"></param>
        /// <param name="fileName">The filename to save the data under</param>
        /// <param name="dataToSave">The object we are saving. Note: Total size of all saved files cannot exceed 12MB</param>
        public IEnumerator SaveAsync<T>(string containerName, string fileName, T dataToSave)
        {
            if (_shouKillAsyncSaves)
            {
                Debug.Log($"{nameof(SaveAsync)} - Killing async save");
                _shouKillAsyncSaves = false;
                yield break;
            }

            var dataAsJson = JsonConvert.SerializeObject(dataToSave);
            var dataBytes = Encoding.ASCII.GetBytes(dataAsJson);
            var saveTask = SaveAsyncImplementation(containerName, fileName, dataBytes);

            yield return new WaitUntil(() => saveTask.IsCompleted);
        }

        /// <summary>
        /// Saves data to the blob (file) within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to save the data to within the specified container.</param>
        /// <param name="blobData">The bytes that are to be written to the blob (file).</param>
        private async Task SaveAsyncImplementation(string containerName, string blobName, byte[] blobData)
        {
            if (_shouKillAsyncSaves)
            {
                Debug.Log($"{nameof(SaveAsyncImplementation)} Killing async save");
                _shouKillAsyncSaves = false;
                return;
            }

            //Step 1: Create a container
            if (!TryCreateSaveContainer(containerName, out var gameSaveContainerHandle))
            {
                return;
            }

            Debug.Log($"Starting save for container {containerName} and blob {blobName} container created");
            //Step 2:  Start container Update
            var containerUpdateResult = SDK.XGameSaveCreateUpdate(gameSaveContainerHandle, blobName, out var gameSaveContainerUpdateHandle);
            if (HR.FAILED(containerUpdateResult))
            {
                SDK.XGameSaveCloseUpdate(gameSaveContainerUpdateHandle);
                Debug.LogError($"Error when creating the {blobName} container display  update process. HResult: ({Unity.XGamingRuntime.HR.NameOf(containerUpdateResult)}  0x{containerUpdateResult:x})");
                return;
            }

            try
            {
                if (_shouKillAsyncSaves)
                {
                    Debug.Log($"{nameof(SaveAsyncImplementation)} Killing async save");
                    _shouKillAsyncSaves = false;
                    return;
                }

                //Step 3: Submit data blob to write
                var submitResult = SDK.XGameSaveSubmitBlobWrite(gameSaveContainerUpdateHandle, blobName, blobData);
                if (HR.FAILED(submitResult))
                {
                    Debug.LogError(
                        $"Error when submitting the blob {blobName}. HResult: ({Unity.XGamingRuntime.HR.NameOf(submitResult)}  0x{submitResult:x})");
                    return;
                }


                if (_shouKillAsyncSaves)
                {
                    Debug.Log($"{nameof(SaveAsyncImplementation)} Killing async save");
                    _shouKillAsyncSaves = false;
                    return;
                }

                //Submit blob updates
                var saveTaskResult = new TaskCompletionSource<bool>();
                SDK.XGameSaveSubmitUpdateAsync(gameSaveContainerUpdateHandle, hResult =>
                {
                    if (HR.FAILED(hResult))
                    {
                        Debug.LogError(
                            $"Error when submitting container updated process. HResult: ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                        saveTaskResult.SetResult(false);
                        return;
                    }

                    saveTaskResult.SetResult(true);
                    Debug.Log($"{blobName} save successful");

                });

                await saveTaskResult.Task;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while saving async: {e}");
            }
            finally
            {
                SDK.XGameSaveCloseUpdate(gameSaveContainerUpdateHandle);
                SDK.XGameSaveCloseContainer(gameSaveContainerHandle);
            }


        }

        private bool TryCreateSaveContainer(string containerName, out XGameSaveContainerHandle gameSaveContainerHandle)
        {
            if (_gameSaveProviderHandle == null)
            {
                Debug.LogError("Game Save Provider not initialized. Not doing anything.");
                gameSaveContainerHandle = null;
                return false;
            }

            var hResult = SDK.XGameSaveCreateContainer(_gameSaveProviderHandle, containerName, out gameSaveContainerHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"Error when creating the {containerName} container. HResult: ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                gameSaveContainerHandle = null;
                return false;
            }

            return true;
        }

    }
}
