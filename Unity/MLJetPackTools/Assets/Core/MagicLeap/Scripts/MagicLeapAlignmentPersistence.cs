using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

#if !XR_NON_MAGIC_LEAP
using UnityEngine.XR.MagicLeap;
#endif

namespace MLJetPack.AlignedXR
{
    /// <summary>
    /// DynamicPersistence example. Demonstrates how to persist objects dynamically by
    /// interfacing with the MLPersistence api and other components in the ML persistence system.
    /// </summary>
	///
	#if !XR_NON_MAGIC_LEAP
	[RequireComponent(typeof(PrivilegeRequester))]
	#endif
    public class MagicLeapAlignmentPersistence : MonoBehaviour
    {
        public static MagicLeapAlignmentPersistence instance = null;

        #region variables
        [Serializable]
        struct ObjIds
        {
            public string[] Ids;
        }

        [SerializeField]
        Text _progressText;

        bool CanPlaceObject
        {
            get
            {
                return _state == State.RestoreComplete || _state == State.Done;
            }
        }

        const string TEXT_RESTORING_OBJECTS = "Restoring objects please wait..";
        const string TEXT_FAILED_TO_START_PERSISTENT_STORE = "Failed to start persistent store. Retrying ..";
        const string TEXT_RESTORING_OBJECT = "Restoring Object : {0} {1}";
        const string RETORE_COMPLETE = "Restore complete";
        const string TEXT_RETRY_PCF = "Cannot Start PCF system due to error: {0}. Please make sure to scan the area around \n you and try again. Retrying in {0} seconds.";
        const string TEXT_SAVE_COMPLETE = "Saved {0} objects!";
        const string FAILED_TO_FIND_CLOSEST_PCF = "Failed to find closest PCF.";
        const string TEXT_ADD_OBJECT = "Ready to add objects (Press Bumper)";
        const string TEXT_FAILED_TO_START_INPUT = "Failed to connect to the control.";
        const int _retryIntervalInSeconds = 3;

		#if !XR_NON_MAGIC_LEAP
        MLContentBinding alignedBinding = null;
		PrivilegeRequester _privilegeRequester;
		#endif

        enum State
        {
            Idle,
            StartRestore,
            StartRestoreObject,
            RestoreInProgress,
            RestoreComplete,
            SaveRequired,
            CritialError,
            Done
        }
        State _state = State.Idle;

        #endregion

        #region functions
        void Awake()
        {
            if (instance == null) {
                instance = this;
            } else {
                Debug.LogError(instance.name + " exists: two spatiate alignment instances. Destroying this (" + name + ")");
                Destroy(gameObject);
                return;
            }

			#if !XR_NON_MAGIC_LEAP
			_privilegeRequester = GetComponent<PrivilegeRequester>();
            if (_privilegeRequester == null)
            {
                Debug.LogError("Missing PrivilegeRequester component");
                enabled = false;
                return;
            }

            // Could have also been set via the editor.
            _privilegeRequester.Privileges = new[] { MLRuntimeRequestPrivilegeId.PwFoundObjRead };
            _privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;
			#endif
        }

		#if !XR_NON_MAGIC_LEAP
        /// <summary>
        /// Responds to privilege requester result.
        /// </summary>
        /// <param name="result"/>
        void HandlePrivilegesDone(MLResult result)
        {
            if (!result.IsOk)
            {
                _state = State.CritialError;
                string errorMsg = string.Format("Privilege Error: {0}", result);
                Debug.LogErrorFormat(errorMsg);
                SetProgress(errorMsg);
                return;
            }

            string message = "Privilege Granted";
            SetProgress(message);
            Debug.LogFormat(message);
            StartRestore();
        }

        /// <summary>
        /// Starts restoration.
        /// </summary>
        void StartRestore()
        {
            // TODO: check for errors
            StartCoroutine(StartPersistenceSystems());
        }
        /// <summary>
        /// Starts the persistence systems, MLPersistentStore and MLPersistentCoordinateFrames
        /// </summary>
        IEnumerator StartPersistenceSystems()
        {
            if (!MLPersistentStore.IsStarted)
            {
                MLResult result = MLPersistentStore.Start();
                if (!result.IsOk)
                {
                    SetProgress(TEXT_FAILED_TO_START_PERSISTENT_STORE);
                    _state = State.CritialError;
                }
                else
                {
                    while(true)
                    {
                        result = MLPersistentCoordinateFrames.Start();

                        if((MLPassableWorldResult)result.Code == MLPassableWorldResult.LowMapQuality 
                            || (MLPassableWorldResult)result.Code == MLPassableWorldResult.UnableToLocalize)
                        {
                            SetProgress(string.Format(TEXT_RETRY_PCF, result, _retryIntervalInSeconds));
                            yield return new WaitForSeconds(_retryIntervalInSeconds);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            OnStartPersistentSystemComplete();
        }
        /// <summary>
        /// Starts the restoration process after the basic systems are initialized.
        /// </summary>
        void OnStartPersistentSystemComplete()
        {
            SetProgress(TEXT_RESTORING_OBJECTS);
            _state = State.StartRestore;
        }

        /// <summary>
        /// Helper function to log error message and update it on the progress
        /// indicator
        /// </summary>
        /// <param name="progressText">Progress text.</param>
        void SetProgress(string progressText)
        {
            Debug.Log("setting progress: " + progressText);
            if (_progressText != null) {
                _progressText.text = progressText;
            }
        }

        /// <summary>
        /// updates the state and handles input
        /// </summary>
        void Update()
        {
            ProcessState();
        }

        /// <summary>
        /// Hands the various states and state transitions.
        /// </summary>
        void ProcessState()
        {
            switch (_state)
            {
                case State.StartRestore:
                    StartRestoration();
                    break;
                case State.RestoreInProgress:
                    _state = State.RestoreComplete;
                    break;
                case State.RestoreComplete:
                    HandleRestoreComplete();
                    break;
                case State.SaveRequired:
                    TrySave();
                    break;
            }
        }

        /// <summary>
        /// Starts the restoration by loading game object bindings. Should
        /// only be called in the State StartRestore.
        /// </summary>
        void StartRestoration()
        {
            LoadBindings();
        }

        public string referencePointName = "alignedReferencePoint";

        /// <summary>
        /// Loads the bindings from the json file
        /// </summary>
        public void LoadBindings()
        {
            _state = State.RestoreInProgress;
            if (!MLPersistentStore.IsStarted)
            {
                return;
            }
            if (MLPersistentStore.Contains(referencePointName))
            {
                MLResult result = MLPersistentStore.Load(referencePointName, out alignedBinding);
                if (!result.IsOk)
                {
                    Debug.LogError("Failed to load binding for reference point");
                }
                else
                {
                    AlignedReferencePoint refPoint = AlignedReferencePoint.instance;
                    alignedBinding.GameObject = refPoint.gameObject;
                    Debug.LogFormat("Binding loaded from the store: " +
                                    "Id: {0} \n" +
                                    "PCFID: {1}\n",
                                    alignedBinding.ObjectId,
                                    alignedBinding.PCF.CFUID);
                    MLContentBinder.Restore(alignedBinding, HandleBindingRestore);
                }
            }
            else
            {
                AlignedReferencePoint.instance.ActionResetXzToTransformPosition(GlobalAppMonitor.mainCamera.transform);
                //SetProgress(string.Format(TEXT_RESTORING_OBJECT, referencePointName, "failed"));
            }
        }

        /// <summary>
        /// Handler for restore complete. After restore is complete we go into the Done state
        /// where you can start adding more objects.
        /// </summary>
        void HandleRestoreComplete()
        {
            SetProgress(TEXT_ADD_OBJECT);
            _state = State.Done;
        }

        /// <summary>
        /// Handler for restoring the bindings. This is called when the content binding
        /// is restored, and if this is successful, the object is rebound to the original
        /// location it was bound to when last saved.
        /// </summary>
        /// <param name="contentBinding">Content binding.</param>
        /// <param name="resultCode">Result code.</param>
        void HandleBindingRestore(MLContentBinding contentBinding, MLResult result)
        {
            if (result.IsOk)
            {
                SetProgress(string.Format(TEXT_RESTORING_OBJECT, contentBinding.GameObject.name, "succeeded"));
                contentBinding.GameObject.SetActive(true);
                Debug.LogFormat("object: {0} - {1} {2} {3} , {4} {5} {6} {7}", contentBinding.GameObject.name, 
                                contentBinding.GameObject.transform.position.x, 
                                contentBinding.GameObject.transform.position.y, 
                                contentBinding.GameObject.transform.position.z,
                                contentBinding.GameObject.transform.rotation.x,
                                contentBinding.GameObject.transform.rotation.y,
                                contentBinding.GameObject.transform.rotation.z,
                                contentBinding.GameObject.transform.rotation.w);
            }
            else
            {
                SetProgress(string.Format(TEXT_RESTORING_OBJECT, contentBinding.GameObject.name, "failed. Trying again"));
                //Note: Currently we try endlessly to restore. However, this can lead to a loop
                //when there's fundamental problem with the underlying system. This logic can be made smarter by
                //adding retry attempts however for sake of simplicitly we are not showing it here. It is advised
                //to do it however.
                MLContentBinder.Restore(contentBinding, HandleBindingRestore);
            }
        }

        /// <summary>
        /// Helper function to add new objects and binding them to closest PCFs.
        /// This function shows how youc an use the underlying systems to accomplish
        /// game object to a PCF binding
        /// </summary>
        public void StoreReferencePointAtPosition(Vector3 position)
        {
            if (!MLPersistentCoordinateFrames.IsStarted)
            {
                return;
            }

            var returnResult = MLPersistentCoordinateFrames.FindClosestPCF(position, (MLResult result, MLPCF pcf) =>
            {
                if (result.IsOk)
                {
                    Debug.LogFormat("Closest PCF found. Binding {0} to PCF {1}:", referencePointName, pcf.CFUID);

                    if (alignedBinding == null) {
                        alignedBinding = MLContentBinder.BindToPCF(referencePointName, AlignedReferencePoint.instance.gameObject, pcf);
                    } else {
                        alignedBinding.PCF = pcf;
                        alignedBinding.Update();
                    }

                    _state = State.SaveRequired;
                    //Debug.LogFormat("object: {0} - {1} {2} {3}, {4} {5} {6} {7}",
                                    //newExampleObject.GO.name,
                                    //newExampleObject.GO.transform.position.x,
                                    //newExampleObject.GO.transform.position.y,
                                    //newExampleObject.GO.transform.position.z,
                                    //newExampleObject.GO.transform.rotation.x,
                                    //newExampleObject.GO.transform.rotation.y,
                                    //newExampleObject.GO.transform.rotation.z,
                                    //newExampleObject.GO.transform.rotation.w);
                }
                else
                {
                    SetProgress(FAILED_TO_FIND_CLOSEST_PCF + " Reason:" + result);
                }
            });

            if (!returnResult.IsOk)
            {
                SetProgress(FAILED_TO_FIND_CLOSEST_PCF + " Result Code:" + returnResult);
            }
        }

        /// <summary>
        /// Tries to save the existing game object ids and also calls the persistent
        /// system save call to ensure game object to the PCF bindings are saved.
        /// </summary>
        void TrySave()
        {
            if (_state == State.SaveRequired && alignedBinding != null)
            {
                _state = State.Done;
                //Debug.LogFormat("Saving Objects {0}", _exampleObjects.Count);
                if (!MLPersistentStore.IsStarted)
                {
                    Debug.LogError("MLPersistentStore is not started! can't save. ");
                    return;
                }

                //Update the binding (re-store offsets) before saving
                alignedBinding.Update();
                Debug.Log("saving binding for: " + referencePointName);
                MLPersistentStore.Save(alignedBinding);

                SetProgress(string.Format(TEXT_SAVE_COMPLETE, 1));
            }
        }

        /// <summary>
        /// Shuts down the started systems.
        /// </summary>
        void OnDestroy()
        {
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
            }
            if (MLPersistentStore.IsStarted)
            {
                MLPersistentStore.Stop();
            }
            if(_privilegeRequester != null)
            {
                _privilegeRequester.OnPrivilegesDone -= HandlePrivilegesDone;
            }
        }
		#endif
		#endregion
    }

}
