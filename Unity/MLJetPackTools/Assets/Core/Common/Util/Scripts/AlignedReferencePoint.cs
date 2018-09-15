using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !XR_NON_MAGIC_LEAP
using UnityEngine.XR.MagicLeap;
#endif

public class ReferencePointData
{
    public Vector3 position;
    public Vector3 eulerAngles;
}

namespace MLJetPack.AlignedXR
{
    public class AlignedReferencePoint : MonoBehaviour
    {
        public static AlignedReferencePoint instance;
        public const string referenceName = "AlignedReferencePoint";
        public GameObject floor;

        public List<GameObject> displayObjects = new List<GameObject>();

        public delegate void FloorEstablishedCallback(AlignedReferencePoint refPoint);
        public FloorEstablishedCallback OnFloorEstablished;

#if !XR_NON_MAGIC_LEAP
        public delegate void CalibrationStatusCallback(AlignedReferencePoint refPoint, bool success, string errorReason = null);
        public CalibrationStatusCallback OnCalibrationAttempted;
# endif

        public GameObject takeMeAlong;
        public void SetPosition(Vector3 position)
        {
            //				Debug.Log ("New ref position: " + position.ToString ("F2"));
            transform.position = position;
            if (takeMeAlong != null)
            {
                takeMeAlong.transform.position = position;
            }
        }

#if !XR_NON_MAGIC_LEAP
        protected RaycastHit GetWorldRaycastResult(MLWorldRays.MLWorldRaycastResultState state, Vector3 point, Vector3 normal, float confidence)
        {
            RaycastHit result = new RaycastHit();

            if (state != MLWorldRays.MLWorldRaycastResultState.RequestFailed && state != MLWorldRays.MLWorldRaycastResultState.NoCollision)
            {
                result.point = point;
                result.normal = normal;
                result.distance = Vector3.Distance(raycastParams.Position, point);
            }
            else
            {
                // Do something else
                result.distance = -1;
            }

            return result;
        }

        MLWorldRays.QueryParams raycastParams =
                       new MLWorldRays.QueryParams();
#endif

        public void ActionResetXzToTransformPosition(Transform resetTransform)
        {
#if !XR_NON_MAGIC_LEAP
            MLResult result = MLWorldRays.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error BaseRaycast starting MLWorldRays, disabling script.");
                MagicLeapResetToTransformPosition(resetTransform.position, resetTransform.forward);
                return;
            }

            raycastParams.Position = resetTransform.position;
            raycastParams.Direction = -Vector3.up;
            raycastParams.Width = 1;
            raycastParams.Height = 1;
            raycastParams.HorizontalFovDegrees = 40;
            raycastParams.CollideWithUnobserved = false;

            MLWorldRays.GetWorldRays(raycastParams, (MLWorldRays.MLWorldRaycastResultState state, Vector3 point, Vector3 normal, float confidence) =>
            {
                RaycastHit hitResult = GetWorldRaycastResult(state, point, normal, confidence);
                if (hitResult.distance < 0)
                {
                    MagicLeapResetToTransformPosition(resetTransform.position, resetTransform.forward);
                    if (OnCalibrationAttempted != null)
                    {
                        OnCalibrationAttempted(this, false, "Please stand on a mesh for more accuracy");
                    }
                }
                else
                {
                    ResetToTransformPositionAndForward(hitResult.point, resetTransform.forward, true);
                    if (OnCalibrationAttempted != null)
                    {
                        OnCalibrationAttempted(this, true);
                    }
                }
                MLWorldRays.Stop();
            });
#else
            ResetToTransformPositionAndForward (resetTransform.position, resetTransform.forward);
#endif
        }

#if !XR_NON_MAGIC_LEAP
        void MagicLeapResetToTransformPosition(Vector3 position, Vector3 forward)
        {
            position.y = GlobalAppMonitor.mainCamera.transform.position.y - 1.4f;

            forward.y = 0;
            ResetToTransformPositionAndForward(position, forward);
        }
#endif

        public void ActionResetXyzToTransformPosition(Transform resetTransform)
        {
            ResetToTransformPositionAndForward(resetTransform.position, resetTransform.forward, true);
        }

        public void ResetToTransformPositionAndForward(Vector3 transformPosition, Vector3 forward, bool updateYPosition = false)
        {
            if (!updateYPosition)
            {
                transformPosition.y = transform.position.y;
            }
            forward.y = 0;


            // DSL: Torn on this one. Not sure if I want to move the anchor point 
            // as well as the ref point or not.
            SetReferencePointByPositionAndRotation(transformPosition, Quaternion.LookRotation(forward, Vector3.up));
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            if (floor == null)
            {
                foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                {
                    if (mr.name.Equals("Plane"))
                    {
                        floor = mr.gameObject;
                    }
                }
            }

            InitializePersistentAnchor();
        }

        void InitializePersistentAnchor()
        {
            // Happens automatically with SpatiateAlignmentPersistence
        }

        public void LoadAnchorFromStore()
        {
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.G))
            {
                SetReferencePointByGameObject();
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                StorePersistentAnchor(objectToAnchor.GetComponent<Transform>());
            }
#endif
        }

#if UNITY_EDITOR
        public GameObject objectToAnchor;
#endif

        public void SetReferencePointByCamera()
        {
            SetReferencePointByGameObject(GlobalAppMonitor.mainCamera.gameObject);
        }

        public void SetReferencePointWithFixedFloorAndUpVector(float floorY, Transform calibrationPoint)
        {
            ClearPersistentAnchor();

            Vector3 calibratePosition = calibrationPoint.transform.position;
            calibratePosition.y = floorY;
            SetPosition(calibratePosition);

            Vector3 forward = calibrationPoint.transform.forward;
            forward.y = 0;
            transform.forward = forward;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            StorePersistentAnchor();
        }

        public void SetReferencePointByPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            ClearPersistentAnchor();
            SetPosition(position);
            transform.rotation = rotation;
            StorePersistentAnchor();
        }

        public void SetReferencePointByGameObject(GameObject referencePoint = null)
        {
            SetReferencePointByGameObjectAndHeightOffset(referencePoint, 0);
        }

        public void SetReferencePointByGameObjectAboveGround6Inches(GameObject referencePoint = null)
        {
            SetReferencePointByGameObjectAndHeightOffset(referencePoint, -.1524f);
        }

        public void SetReferencePointByGameObjectAndHeightOffset(GameObject referencePoint, float heightOffset)
        {
            ClearPersistentAnchor();

            if (referencePoint != null)
            {
                Vector3 position = referencePoint.transform.position;
                position.y += heightOffset;
                SetPosition(position);

                Vector3 forward = referencePoint.transform.forward;
                forward.y = 0;
                transform.forward = forward;
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

                StorePersistentAnchor();
                return;
            }

            GameObject gc = GameObject.Find("GameController");
            if (referencePoint != null)
            {
                if (gc != null)
                {
                    SetPosition(referencePoint.transform.position);

                    Vector3 forward = referencePoint.transform.forward;
                    forward.y = 0;
                    transform.forward = forward;
                    transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    return;
                }
            }

            StorePersistentAnchor();
        }

        void ClearPersistentAnchor()
        {
        }

        public void StorePersistentAnchor(Component existingAnchor = null)
        {
#if !XR_MAGIC_LEAP
            if (MagicLeapAlignmentPersistence.instance != null)
            {
                MagicLeapAlignmentPersistence.instance.StoreReferencePointAtPosition(transform.position);
            }
#endif
        }

        public void SetDisplayObjectsActive(bool shouldBeActive)
        {
            //				Debug.Log ("Setting display objects active: " + shouldBeActive); 
            foreach (GameObject go in displayObjects)
            {
                go.SetActive(shouldBeActive);
            }
        }

        public void EstablishFloor(Transform centerPoint)
        {
            if (floor != null)
            {
                SetPosition(centerPoint.position);
                transform.rotation = centerPoint.rotation;
                floor.transform.position = centerPoint.position;
            }
            //				Debug.Log ("Establishing floor");
            if (OnFloorEstablished != null)
            {
                //					Debug.Log ("Establishing floor callback");
                OnFloorEstablished(this);
            }
        }
    }
}