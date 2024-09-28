using Unity.XGamingRuntime;
using UnityEngine;
using HR = Unity.XGamingRuntime.Interop.HR;

namespace Assets._Scripts.Xbox
{
    public class XboxSavedDataHandler
    {
        private XUserHandle _userHandle;
        private XGameSaveContainerHandle _gameSaveContainerHandle;
        private XGameSaveProviderHandle _gameSaveProviderHandle;
        private XGameSaveUpdateHandle _gameSaveContainerUpdateHandle;

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

            var hr = SDK.XUserDuplicateHandle(userHandle, out _userHandle);
            if (HR.FAILED(hr))
            {
                return false;
            }

            var result = SDK.XGameSaveInitializeProvider(_userHandle, scid, false, out _gameSaveProviderHandle);
            if (HR.FAILED(result))
            {
                Debug.LogError($"FAILED: Initialize game save provider");
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

            if(HR.FAILED(blobInfoResult))
            {
                Debug.LogError($"FAILED: Enumerate blob info by name. HResult: 0x{blobInfoResult:x}");
                return default;
            }

            //Step 3: Read the blob 
            var readBlobResult = SDK.XGameSaveReadBlobData(_gameSaveContainerHandle,
                blobInfos,
                out var data
            );

            if (!HR.FAILED(readBlobResult)) return data[0].Data;

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
                Debug.LogError($"Error when creating the {blobName} container display  update process. HResult: 0x{containerUpdateResult:x}");
                return false;
            }

            //Step 3: Submit data blob to write
            var submitResult = SDK.XGameSaveSubmitBlobWrite(_gameSaveContainerUpdateHandle, blobName, blobData);
            if (HR.FAILED(submitResult))
            {
                Debug.LogError($"Error when submitting the blob {blobName}. HResult: 0x{submitResult:x}");
                return false;
            }

            //Submit blob update
            var hResult = SDK.XGameSaveSubmitUpdate(_gameSaveContainerUpdateHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"Error when submitting container updated process. HResult: 0x{hResult:x}");
                return false;
            }

            Debug.Log($"{containerName} | {blobName} Save completed. Closing Update handle and container.");
            SDK.XGameSaveCloseUpdate(_gameSaveContainerUpdateHandle);
            SDK.XGameSaveCloseContainer(_gameSaveContainerHandle);

            return true;

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
                Debug.LogError($"Error when creating the {containerName} container. HResult: 0x{hResult:x}");
                return false;
            }

            return true;
        }

    }
}
