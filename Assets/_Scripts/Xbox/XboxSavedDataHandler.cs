using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public bool IsProcessingSave { get; set; }
        public Queue<IEnumerator> SaveQueue { get; private set; } = new();

        private XUserHandle _userHandle;
        private XGameSaveContainerHandle _gameSaveContainerHandle;
        private XGameSaveProviderHandle _gameSaveProviderHandle;
        private XGameSaveUpdateHandle _gameSaveContainerUpdateHandle;
        private ConcurrentDictionary<int, bool> _inProgressSaveIndices = new();
        private const int RollingFileMax = 5;
        private static int _rollingFileIndex = 1;


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
            for (var i = 1; i <= RollingFileMax; i++)
            {
                _inProgressSaveIndices.AddOrUpdate(i, _ => false, (_, _) => false);
            }

            SaveQueue = new Queue<IEnumerator>();

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

            Debug.Log("SUCCESS: Initialize game save provider");
            return true;
        }

        /// <summary>
        /// Loads the data from a given blob (file) that is within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to load data from.</param>
        public byte[] Load(string containerName, string blobName)
        {
            //Step 1: Create a container
            if (!TryCreateSaveContainer(containerName))
            {
                return default;
            }

            //Step 2: Get metadata about the blob
            var blobInfoResult = SDK.XGameSaveEnumerateBlobInfoByName(_gameSaveContainerHandle, blobName, out var blobInfos);

            if (HR.FAILED(blobInfoResult))
            {
                Debug.LogError($"FAILED: Enumerate blob info by name. HResult: ({Unity.XGamingRuntime.HR.NameOf(blobInfoResult)}  0x{blobInfoResult:x})");
                return default;
            }

            //Step 3: Read the blob 
            var readBlobResult = SDK.XGameSaveReadBlobData(_gameSaveContainerHandle,
                blobInfos,
                out var data
            );

            if (HR.FAILED(readBlobResult))
            {
                if (readBlobResult == Unity.XGamingRuntime.HR.E_GS_USER_NOT_REGISTERED_IN_SERVICE)
                {
                    Debug.LogError($"FAILED: User may be logging out x{readBlobResult:X} ({Unity.XGamingRuntime.HR.NameOf(readBlobResult)})");
                }
                else
                {
                    Debug.LogError($"FAILED: Game load, hResult=0x{readBlobResult:X} ({Unity.XGamingRuntime.HR.NameOf(readBlobResult)})");
                }

                return default;
            }


            if (data.Length == 1) return data[0].Data;

            //Step 4: Read the metadata
            var metaBlobInfoResult = SDK.XGameSaveEnumerateBlobInfoByName(_gameSaveContainerHandle, "META", out var metaBlobInfos);
            if (HR.FAILED(metaBlobInfoResult))
            {
                Debug.LogError($"FAILED: Enumerate blob info by name for metadata. HResult: ({Unity.XGamingRuntime.HR.NameOf(metaBlobInfoResult)}  0x{metaBlobInfoResult})");
                return default;
            }
            var metaReadResult = SDK.XGameSaveReadBlobData(_gameSaveContainerHandle,
                metaBlobInfos,
                out var metaBlobData
            );

            if (HR.FAILED(metaReadResult))
            {
                Debug.LogError($"FAILED: Read metadata blob. HResult: ({Unity.XGamingRuntime.HR.NameOf(metaReadResult)}  0x{metaReadResult:x})");
                return default;
            }

            var loadedDataAsJson = Encoding.ASCII.GetString(metaBlobData[0].Data);
            Debug.Log($"Save METADATA: {loadedDataAsJson}");
            var metadata = JsonConvert.DeserializeObject<XboxSaveMetadata>(loadedDataAsJson);
            var recentData = data.FirstOrDefault(x => x.Info.Name.EndsWith(metadata.LastSaveIndex.ToString()));
            return recentData?.Data;
        }

        /// <summary>
        /// Saves data to the blob (file) within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to save the data to within the specified container.</param>
        /// <param name="blobData">The bytes that are to be written to the blob (file).</param>
        public bool Save(string containerName, string blobName, byte[] blobData)
        {
            //Step 1: Create a container
            if (!TryCreateSaveContainer(containerName))
            {
                return false;
            }

            //Step 2:  Start container Update
            var containerUpdateResult = SDK.XGameSaveCreateUpdate(_gameSaveContainerHandle, blobName, out _gameSaveContainerUpdateHandle);
            if (HR.FAILED(containerUpdateResult))
            {
                Debug.LogError($"Error when creating the {blobName} container display  update process. HResult: ({Unity.XGamingRuntime.HR.NameOf(containerUpdateResult)}  0x{containerUpdateResult:x})");
                return false;
            }

            //Step 3: Submit data blob to write
            var submitResult = SDK.XGameSaveSubmitBlobWrite(_gameSaveContainerUpdateHandle, blobName, blobData);
            if (HR.FAILED(submitResult))
            {
                Debug.LogError($"Error when submitting the blob {blobName}. HResult: ({Unity.XGamingRuntime.HR.NameOf(submitResult)}  0x{submitResult:x})");
                return false;
            }

            //Submit blob update
            var hResult = SDK.XGameSaveSubmitUpdate(_gameSaveContainerUpdateHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"Error when submitting container updated process. HResult: ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                return false;
            }

            Debug.Log($"{containerName} | {blobName} Save completed. Closing Update handle and container.");
            SDK.XGameSaveCloseUpdate(_gameSaveContainerUpdateHandle);
            SDK.XGameSaveCloseContainer(_gameSaveContainerHandle);

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
            var dataAsJson = JsonConvert.SerializeObject(dataToSave);
            var dataBytes = Encoding.ASCII.GetBytes(dataAsJson);
            Debug.Log("About to fire and forget save async");
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
            Debug.Log($"Async Save: [{containerName} | {blobName}] starting.");
            //Step 1: Create a container
            if (!TryCreateSaveContainer(containerName))
            {
                return;
            }

            Debug.Log($"Save: [{containerName} | {blobName}] starting.");

            //Step 2:  Start container Update
            var containerUpdateResult = SDK.XGameSaveCreateUpdate(_gameSaveContainerHandle, blobName, out _gameSaveContainerUpdateHandle);
            if (HR.FAILED(containerUpdateResult))
            {
                Debug.LogError($"Error when creating the {blobName} container display  update process. HResult: ({Unity.XGamingRuntime.HR.NameOf(containerUpdateResult)}  0x{containerUpdateResult:x})");
                return;
            }

            _rollingFileIndex = (_rollingFileIndex + 1) % RollingFileMax;
            var myIndex = _rollingFileIndex;

            blobName = $"{blobName}_{myIndex}";

            //If we're currently writing to this file, wait until it's done
            while (_inProgressSaveIndices[myIndex])
            {
                await Task.Delay(100);
                Debug.Log($"Save: [{containerName} | {blobName}] delayed.");
            }

            _inProgressSaveIndices[myIndex] = true;


            try
            {
                //Step 3: Submit data blob to write
                var submitResult = SDK.XGameSaveSubmitBlobWrite(_gameSaveContainerUpdateHandle, blobName, blobData);
                if (HR.FAILED(submitResult))
                {
                    Debug.LogError(
                        $"Error when submitting the blob {blobName}. HResult: ({Unity.XGamingRuntime.HR.NameOf(submitResult)}  0x{submitResult:x})");
                    return;
                }

                //Step 4: Submit metadata blob to write
                var metaData = new XboxSaveMetadata { LastSaveIndex = myIndex, LastUpdated = DateTime.UtcNow };
                var dataAsJson = JsonConvert.SerializeObject(metaData);
                var dataBytes = Encoding.ASCII.GetBytes(dataAsJson);
                var metaSubmitResult = SDK.XGameSaveSubmitBlobWrite(_gameSaveContainerUpdateHandle, "META", dataBytes);
                if (HR.FAILED(metaSubmitResult))
                {
                    Debug.LogError(
                        $"Error when submitting the metadata blob {blobName}. HResult: ({Unity.XGamingRuntime.HR.NameOf(metaSubmitResult)}  0x{metaSubmitResult:x})");
                    return;
                }

                //Submit blob updates
                var saveTaskResult = new TaskCompletionSource<bool>();
                SDK.XGameSaveSubmitUpdateAsync(_gameSaveContainerUpdateHandle, hResult =>
                {
                    if (HR.FAILED(hResult))
                    {
                        Debug.LogError(
                            $"Error when submitting container updated process. HResult: ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                        saveTaskResult.SetResult(false);
                        return;
                    }


                    Debug.Log($"Save: [{containerName} | {blobName}] completed. Closing Update handle and container.");
                    SDK.XGameSaveCloseUpdate(_gameSaveContainerUpdateHandle);
                    SDK.XGameSaveCloseContainer(_gameSaveContainerHandle);
                    saveTaskResult.SetResult(true);
                });

                await saveTaskResult.Task;

                Debug.Log($"Async Save: [{containerName} | {blobName}] done.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while saving async: {e}");
            }
            finally
            {
                _inProgressSaveIndices[myIndex] = false;
            }

        }

        ///// <summary>
        ///// Callback invoked when the QueryContainerBlobs async task completes.
        ///// </summary>
        ///// <param name="hresult">The hresult of the operation.</param>
        //public delegate void DeleteCallback(int hresult);

        ///// <summary>
        ///// Deletes a container along with all of its blobs (files).
        ///// </summary>
        ///// <param name="containerName">Name of the container to delete.</param>
        ///// <param name="callback">Callback invoked when the async task completes. DeleteCallback(Int32 hresult)</param>
        //public void Delete(string containerName, DeleteCallback callback)
        //{
        //    SDK.XGameSaveDeleteContainerAsync(_gameSaveProviderHandle, containerName,
        //        new XGameSaveDeleteContainerCompleted(callback));
        //}

        ///// <summary>
        ///// Deletes a specific blob (file) from within the specified container.
        ///// </summary>
        ///// <param name="containerName">Name of the container.</param>
        ///// <param name="blobName">Name of the blob (file) to delete from the specified container.</param>
        ///// <param name="callback">Callback invoked when the async task completes. DeleteCallback(Int32 hresult)</param>
        //public void Delete(string containerName, string blobName, DeleteCallback callback)
        //{
        //    Delete(containerName, new[] { blobName }, callback);
        //}

        ///// <summary>
        ///// Deletes a specific set of blobs (files) from within the specified container.
        ///// </summary>
        ///// <param name="containerName">Name of the container.</param>
        ///// <param name="blobNames">Array of blob (file) names to delete from the specified container.</param>
        ///// <param name="callback">Callback invoked when the async task completes. DeleteCallback(Int32 hresult)</param>
        //public void Delete(string containerName, string[] blobNames, DeleteCallback callback)
        //{
        //    Update(containerName, null, blobNames, new UpdateCallback(callback));
        //}


        private bool TryCreateSaveContainer(string containerName)
        {
            if (_gameSaveProviderHandle == null)
            {
                Debug.LogError("Game Save Provider not initialized. Not doing anything.");
                return false;
            }

            var hResult = SDK.XGameSaveCreateContainer(_gameSaveProviderHandle, containerName, out _gameSaveContainerHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"Error when creating the {containerName} container. HResult: ({Unity.XGamingRuntime.HR.NameOf(hResult)}  0x{hResult:x})");
                return false;
            }

            return true;
        }

    }
}
