using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if !XR_NON_MAGIC_LEAP
using UnityEngine.XR.MagicLeap;
#endif

namespace MLJetPack.Interactions
{
    public class MagicLeapHand : MonoBehaviour, ControllerImplementation
    {
        public Text textPoseInfo;

        public enum Handedness
        {
            Left,
            Right
        }

        public Handedness handedness;

        // If there are hand renderers, use them here
        public List<GameObject> handObjects = new List<GameObject>();
        void SetHandObjectsActive(bool shouldBeEnabled) {
            foreach(GameObject handObject in handObjects) {
                //handObject.SetActive(shouldBeEnabled);
            }
        }

		#if !XR_NON_MAGIC_LEAP
		public MLHand mlHand
        {
            get {
                return handedness == Handedness.Left ? MLHands.Left : MLHands.Right;
            }
        }

		#endif

        public enum GestureDetectionFrameState
        {
            NotDetected,
            GestureBegan,
            GestureHeld,
            GestureReleased
        }

		public enum HandCommand {
			ThumbClick
		}

		public bool thumbClickedToState = false;
		public bool thumbClickState = false;

		public void SetThumbClicked(bool isClicked)
		{
			thumbClickedToState = true;
			thumbClickState = isClicked;
		}

#if UNITY_EDITOR
        [SerializeField]
        MLHandKeyPose editorPose = MLHandKeyPose.NoHand;

        [SerializeField]
        bool sendEditorPose = false;
#endif
        #if !XR_NON_MAGIC_LEAP
    public bool isDetected {
            get
            {
                return currentPose != MLHandKeyPose.NoHand;
            }
        }

        public bool hasPose
        {
            get
            {
                return currentPose != MLHandKeyPose.NoHand && currentPose != MLHandKeyPose.NoPose;
            }
        }

        // Manage the current pose
        public MLHandKeyPose currentPose;
        public Dictionary<MLHandKeyPose, GestureDetectionFrameState> gestureState = new Dictionary<MLHandKeyPose, GestureDetectionFrameState>();

        [SerializeField]
        List<MLHandKeyPose> posesToRemove = new List<MLHandKeyPose>();
        [SerializeField]
        List<MLHandKeyPose> posesToHold = new List<MLHandKeyPose>();
        public void ResetFrame()
        {
            posesToRemove.Clear();
            posesToHold.Clear();

            foreach (KeyValuePair<MLHandKeyPose, GestureDetectionFrameState> pair in gestureState)
            {
                switch (pair.Value)
                {
                    case GestureDetectionFrameState.GestureBegan:
                    case GestureDetectionFrameState.GestureHeld:
                        posesToHold.Add(pair.Key);
                        break;
                    case GestureDetectionFrameState.GestureReleased:
                    case GestureDetectionFrameState.NotDetected:
                        posesToRemove.Add(pair.Key);
                        break;
                }
            }

            foreach (MLHandKeyPose pose in posesToHold) {
                gestureState[pose] = GestureDetectionFrameState.GestureHeld;
            }

            foreach (MLHandKeyPose pose in posesToRemove)
            {
                gestureState.Remove(pose);
            }         }
         //public List<MLFinger> fingerArray = new List<MLFinger>();
        //public List<GameObject> fingerFeatures = new List<GameObject>(); 
        void Awake()
        {
            //fingerArray.Add(hand.Index);
            //fingerArray.Add(hand.Middle);
            //fingerArray.Add(hand.Ring);
            //fingerArray.Add(hand.Pinky);
        }

        void Start()
        {
            //Debug.Log("Hand START: " + hand);
            if (MLHands.IsStarted) {
                mlHand.OnKeyPoseBegin += Hand_OnKeyPoseBegin;
                mlHand.OnKeyPoseEnd += Hand_OnKeyPoseEnd;
            } 
            SetHandObjectsActive(false);
        }

        public delegate void HandPoseCallback(MagicLeapHand hand, MLHandKeyPose newPose);

        public HandPoseCallback OnHandPoseBegin;
        public HandPoseCallback OnHandPoseEnd;

        float lastPoseTime = 0;
        void Hand_OnKeyPoseBegin(MLHandKeyPose obj) {
            if (MLHands.IsStarted && mlHand.HandConfidence < 0.85f)
            {
                return;
            }

            //Debug.Log(name + ": KEY POSE BEGIN: " + obj);
            SetHandObjectsActive(true);

            MLHandKeyPose lastPose = currentPose;
            currentPose = obj;

            if (currentPose != MLHandKeyPose.NoHand && 
                (!MLHands.IsStarted || mlHand.HandConfidence > 0.85f))
            {
                SetHandObjectsActive(true);
            }

            if (textPoseInfo != null)
            {
                textPoseInfo.text = "POSE: " + currentPose.ToString();
                Debug.Log(textPoseInfo.text);
            }

            float poseTime = Time.time - lastPoseTime;
            //Debug.Log("Check for onhandposebegin: " + OnHandPoseBegin);
            if (OnHandPoseBegin != null) {
                //Debug.Log("On hand pose begin: " + obj);
                OnHandPoseBegin(this, obj);
            }
            lastPoseTime = Time.time;
        }

        void Hand_OnKeyPoseEnd(MLHandKeyPose obj) {
            if (obj == MLHandKeyPose.NoHand) {
                SetHandObjectsActive(false);
            }
            if (OnHandPoseEnd != null)
            {
                OnHandPoseEnd(this, obj);
            }
            //Debug.Log("KEY POSE END: " + obj);
            return;

            MLHandKeyPose lastPose = currentPose;
            //if (currentPose == obj)
            //{
            //    currentPose = MLHandKeyPose.NoHand;
            //}

            if (currentPose == MLHandKeyPose.NoHand)
            {
                SetHandObjectsActive(false);
                //textPoseInfo.text = "END POSE: " + obj.ToString();
                //Debug.Log(textPoseInfo.text);
            }

            float poseTime = Time.time - lastPoseTime;
            if (OnHandPoseEnd != null)
            {
                OnHandPoseEnd(this, obj);
            }
            lastPoseTime = Time.time;

        }

        private void OnDisable()
        {
            thumbClickState = false;
        }          void OnDestroy()
        {
            if (MLHands.IsStarted) {
                mlHand.OnKeyPoseBegin -= Hand_OnKeyPoseBegin;
                mlHand.OnKeyPoseEnd -= Hand_OnKeyPoseEnd;
            }
        }

        void Update() {

#if UNITY_EDITOR
            if (sendEditorPose)
            {
                Hand_OnKeyPoseEnd(currentPose);
                Hand_OnKeyPoseBegin(editorPose);
                sendEditorPose = false;
            }
#endif
            if (MLHands.IsStarted) {
                if (currentPose != MLHandKeyPose.NoHand)
                {
                    transform.position = mlHand.Center;
                    Vector3 forward = mlHand.Center - GlobalAppMonitor.mainCamera.transform.position;
                    forward.y = 0;
                    transform.forward = forward;
                }
            }
        }

        // LateUpdate - so we know we are after button callbacks?
        void LateUpdate()
        {
            thumbClickedToState = false;

            // Update gesture states for this frame
            //_controller.updateGestureStates();

            //// InfoState - display and page through Info screens
            //if (_controller.trigger) {
            //    if (!_triggerDown) {
            //    }
            //    _triggerDown = true;
            //}
            //else {
            //    _triggerDown = false;
            //}

            // Reset controller states
            ResetFrame();
        }
		#endif

        // Treat a "button" as a gesture in this case
        public bool ActionButtonActive(int button)
        {
            switch ((HandCommand)button)
            {
                case HandCommand.ThumbClick:
                    return thumbClickState && !thumbClickedToState;
            }
            return false;

#if !XR_NON_MAGIC_LEAP
            return gestureState.ContainsKey((MLHandKeyPose)button);
#else
            return false;
#endif
        }

		public bool ActionButtonIdle (int button) {
            switch ((HandCommand)button)
            {
                case HandCommand.ThumbClick:
                    return !thumbClickState && !thumbClickedToState;
            }
            return false;

            #if !XR_NON_MAGIC_LEAP
            //if (OVRInput.Get((OVRInput.Button)button)) {
            //	return false;
            //}
#endif // #if !XR_NON_MAGIC_LEAP
            return true;
		}

        int lastDownFrameCheck;
        int lastUpFrameCheck;

        public enum CommandFrameState
        {
            Idle,
            ClickedDown,
            ClickedUp,
            ClickHeld
        }

        public bool ActionButtonUp (int button) {
            switch ((HandCommand)button)
            {
                case HandCommand.ThumbClick:
                    return !thumbClickState && thumbClickedToState;
            }
            return false;

#if !XR_NON_MAGIC_LEAP
            MLHandKeyPose pose = (MLHandKeyPose)button;
            if (mlHand.GetKeyPoseUp(pose))
            {
                gestureState[pose] = GestureDetectionFrameState.GestureReleased;
                return true;
            }
#endif
            //if (OVRInput.GetUp((OVRInput.Button)button)) {
            //	return true;
            //}
            return false;
		}

		public bool ActionButtonDown (int button) {
            switch ((HandCommand)button)
            {
                case HandCommand.ThumbClick:
                    return thumbClickState && thumbClickedToState;
            }
            return false;

            #if !XR_NON_MAGIC_LEAP
            MLHandKeyPose pose = (MLHandKeyPose)button;
            //Debug.Log("Look for action button down: " + pose);
            if (mlHand.GetKeyPoseDown(pose))
            {
                gestureState[pose] = GestureDetectionFrameState.GestureBegan;
                //Debug.Log("YESS action button down: " + pose);
                return true;
            }
#endif // #if !XR_NON_MAGIC_LEAP
            return false;
		}

		public bool ActionButtonHeld (int button) {
            switch ((HandCommand)button)
            {
                case HandCommand.ThumbClick:
                    return thumbClickState && !thumbClickedToState;
            }
            return false;

#if !XR_NON_MAGIC_LEAP
            //if (OVRInput.Get((OVRInput.Button)button)) {
            //	return true;
            //}
            return gestureState.ContainsKey((MLHandKeyPose)button);
#else
            return false;
#endif // #if XR_OCULUS_MOBILE
		}

		public bool InteractionFunctionsAreEnabled() {
			return true;
		}

        public bool ActionTouchPad(TouchPadPhase phase, int button, out Vector2 xy)
        {
            xy = Vector2.zero;
#if !XR_NON_MAGIC_LEAP
            //xy = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);

            //switch (phase) {
            //case TouchPadPhase.TouchBegan:
            //	return OVRInput.GetDown (OVRInput.Touch.PrimaryTouchpad);

            //case TouchPadPhase.TouchHeld:
            //	return OVRInput.Get (OVRInput.Touch.PrimaryTouchpad);

            //case TouchPadPhase.TouchEnded:
            //	return OVRInput.GetUp (OVRInput.Touch.PrimaryTouchpad);

            //case TouchPadPhase.ClickDown:
            //	return OVRInput.GetDown (OVRInput.Button.PrimaryTouchpad);

            //case TouchPadPhase.ClickUp:
            //	return OVRInput.GetUp (OVRInput.Button.PrimaryTouchpad);

            //case TouchPadPhase.ClickHeld:
            //	return OVRInput.Get (OVRInput.Button.PrimaryTouchpad);

            //default:
            //	break;
            //}
#else
#endif // #if !XR_NON_MAGIC_LEAP
            return false;
        }

		public void PulseHaptic (float vibeStrength) {
		}

		public bool Simulates6DofControllerConversion(Vector3 previousPosition, Vector3 currentPosition, out Quaternion rotation) {
			rotation = Quaternion.identity;
			return false;
		}

	}
}