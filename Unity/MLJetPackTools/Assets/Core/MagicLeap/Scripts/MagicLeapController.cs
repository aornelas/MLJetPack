using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !XR_NON_MAGIC_LEAP
using UnityEngine.XR.MagicLeap;
#endif


namespace MLJetPack.Interactions
{
    public class MagicLeapController : MonoBehaviour, ControllerImplementation
    {
        public static MagicLeapController instance = null;
        public enum Control
        {
            Trigger = 0,
            Bumper,
            Home,
            Max
        }

        public enum ButtonFrameState
        {
            Unchanged,
            ClickedDown,
            HeldDown,
            ClickedUp
        }

        public enum GestureType
        {
            None,
            RadialScroll
        }

#if !XR_NON_MAGIC_LEAP
        public MLInputController _controller;
#endif         private float _TRIGGER_THRESHOLD = 0.2f;

        public ButtonFrameState[] buttonFrameState = new ButtonFrameState[(int)Control.Max];         public bool[] buttonDownState = new bool[(int)Control.Max];

        public delegate void MagicLeapControllerButtonStateCallback(MagicLeapController controller, Control buttonId, ButtonFrameState buttonFrameState);
        public MagicLeapControllerButtonStateCallback OnMagicLeapControllerButtonStateChange;

        public delegate void MagicLeapControllerTriggerStateCallback(MagicLeapController controller, float triggerValue, ButtonFrameState buttonFrameState);
        public MagicLeapControllerTriggerStateCallback OnMagicLeapControllerTriggerStateChange;

        public delegate void MagicLeapControllerTouchpadGestureEndCallback(MagicLeapController controller, GestureType gesture, Dictionary<string,object> gestureParams);
        public MagicLeapControllerTouchpadGestureEndCallback OnMagicLeapControllerTouchpadGestureEnded;
         public void ResetFrame()
        {
            for (int i = 0; i < buttonFrameState.Length; i++)
            {
                if (buttonDownState[i])
                {
                    buttonFrameState[i] = ButtonFrameState.HeldDown;
                }
                else
                {
                    buttonFrameState[i] = ButtonFrameState.Unchanged;
                }
            }         }          void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }         }

#if !XR_NON_MAGIC_LEAP
        [SerializeField]
        MLInput.Hand hand = MLInput.Hand.Left;

        void Start()
        {
            if (!MLInput.IsStarted) {
                return;
            }             _controller = MLInput.GetController(hand);
#if DEBUG_ML_CONTROLLER
            Debug.Log("Controller: " + _controller);
#endif             MLInput.OnControllerButtonDown += MLInput_OnButtonDown;             MLInput.OnControllerButtonUp += MLInput_OnButtonUp;

            MLInput.OnControllerTouchpadGestureEnd += MLInput_OnTouchpadGestureEnd;

            MLInput.OnTriggerDown += MLInput_OnTriggerDown;
            MLInput.OnTriggerUp += MLInput_OnTriggerUp;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            if (!MLInput.IsStarted)
            {
                return;
            }

            MLInput.OnControllerButtonDown -= MLInput_OnButtonDown;
            MLInput.OnControllerButtonUp -= MLInput_OnButtonUp;

            MLInput.OnControllerTouchpadGestureEnd -= MLInput_OnTouchpadGestureEnd;

            MLInput.OnTriggerDown -= MLInput_OnTriggerDown;
            MLInput.OnTriggerUp -= MLInput_OnTriggerUp;
        }

        public void SetTriggerThresholds(float thresholdDown, float thresholdUp)
        {
            if (!MLInput.IsStarted)
            {
                return;
            }
            MLInput.TriggerDownThreshold = thresholdDown;
            MLInput.TriggerUpThreshold = thresholdUp;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Equals))
            {
                if (OnMagicLeapControllerButtonStateChange != null)
                {
                    OnMagicLeapControllerButtonStateChange(this, Control.Home, ButtonFrameState.ClickedDown);
                }
            }
        }
        // LateUpdate - so we know we are after button callbacks?
        void LateUpdate()
        {
#if DEBUG_ML_CONTROLLER
            //Debug.Log(Time.frameCount + ": Resetting frame");
#endif
            if (!MLInput.IsStarted)
            {
                return;
            }

            if (_controller != null)
            {
                triggerValue = _controller.TriggerValue;
            }
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

            if (_controller != null)
            {
                if (_controller.TriggerValue >= _TRIGGER_THRESHOLD)
                {
                    if (buttonDownState[(int)Control.Trigger] == false)
                    {
                        //Debug.Log("Clicked Down");
                        buttonFrameState[(int)Control.Trigger] = ButtonFrameState.ClickedDown;
                    }
                    else
                    {
                        //Debug.Log("Already had a click down");
                    }
                    buttonDownState[(int)Control.Trigger] = true;

                }
                else
                { //_controller.TriggerValue < _TRIGGER_THRESHOLD
                    if (buttonDownState[(int)Control.Trigger] == true)
                    {
                        buttonFrameState[(int)Control.Trigger] = ButtonFrameState.ClickedUp;
                        //Debug.Log("Clicked Up");
                    }
                    buttonDownState[(int)Control.Trigger] = false;
                }
            }
        }

        void MLInput_OnButtonDown(byte controller_id, MLInputControllerButton button)
        {
            //Debug.Log("On button down: " + controller_id + " (mine is " + _controller.Id + ")");
            if (controller_id != _controller.Id) {
                //Debug.Log("Not mine, return");
                return;
            }

            int buttonId = -1;
            switch (button)
            {
                case MLInputControllerButton.Bumper:
                    buttonId = (int)Control.Bumper;
                    break;

                case MLInputControllerButton.HomeTap:
                    buttonId = (int)Control.Home;
                    break;
            }

            Debug.Log("Button id is " + buttonId);

            if (buttonId != -1)
            {
                buttonFrameState[buttonId] = ButtonFrameState.ClickedDown;
                buttonDownState[buttonId] = true;

#if DEBUG_ML_CONTROLLER
                Debug.Log(Time.frameCount + ": Setting frame state - " + (Control)buttonId + " to down");
#endif
                Debug.Log("Do the callback");
                if (OnMagicLeapControllerButtonStateChange != null)
                {
                    OnMagicLeapControllerButtonStateChange(this, (Control)buttonId, ButtonFrameState.ClickedDown);
                }
                else
                {
                    Debug.LogError("Not catching a button: " + buttonId);
                }
            }
        }

        private GestureType ConvertedGesture(MLInputControllerTouchpadGestureType gesture) {
            switch (gesture) {
                case MLInputControllerTouchpadGestureType.ForceDwell:
                    break;

                case MLInputControllerTouchpadGestureType.ForceTapDown:
                    break;

                case MLInputControllerTouchpadGestureType.ForceTapUp:
                    break;

                case MLInputControllerTouchpadGestureType.LongHold:
                    break;

                case MLInputControllerTouchpadGestureType.Pinch:
                    break;

                case MLInputControllerTouchpadGestureType.RadialScroll:
                    return GestureType.RadialScroll;

                case MLInputControllerTouchpadGestureType.Scroll:
                    break;

                case MLInputControllerTouchpadGestureType.SecondForceDown:
                    break;

                case MLInputControllerTouchpadGestureType.Swipe:
                    break;

                case MLInputControllerTouchpadGestureType.Tap:
                    break;
            }
            return GestureType.None;
        }

        private void MLInput_OnTouchpadGestureEnd(byte controller_id, MLInputControllerTouchpadGesture gesture)
        {
            Dictionary<string, object> gestureParams = new Dictionary<string, object>();
            if (gesture.Type.Equals(MLInputControllerTouchpadGestureType.RadialScroll))
            {
                gestureParams["gesture"] = gesture;
                if (OnMagicLeapControllerTouchpadGestureEnded != null) {
                    OnMagicLeapControllerTouchpadGestureEnded(this, ConvertedGesture(gesture.Type), gestureParams);
                }
            }
        }
         void MLInput_OnButtonUp(byte controller_id, MLInputControllerButton button)
        {
            //Debug.Log("On button up: " + controller_id + " (mine is " + _controller.Id + ")");
            if (controller_id != _controller.Id)
            {
                //Debug.Log("Not mine, return");
                return;
            }

            int buttonId = -1;
            switch (button)
            {
                case MLInputControllerButton.Bumper:
                    buttonId = (int)Control.Bumper;
                    break;

                case MLInputControllerButton.HomeTap:
                    buttonId = (int)Control.Home;
                    break;
            }

            Debug.Log("Button id is " + buttonId);

            if (buttonId != -1)
            {
                buttonFrameState[buttonId] = ButtonFrameState.ClickedUp;
                buttonDownState[buttonId] = false;
#if DEBUG_ML_CONTROLLER
                Debug.Log(Time.frameCount + ": Setting frame state - " + (Control)buttonId + " to up");
#endif

                Debug.Log("Do the callback");
                if (OnMagicLeapControllerButtonStateChange != null)
                {
                    OnMagicLeapControllerButtonStateChange(this, (Control)buttonId, ButtonFrameState.ClickedUp);
                }
            }
            else
            {
                Debug.LogError("Not catching a button: " + buttonId);
            }
        }

        void MLInput_OnTriggerUp(byte controller_id, float pressure)
        {
            if (controller_id != _controller.Id)
            {
                //Debug.Log("Not mine, return");
                return;
            }

            if (OnMagicLeapControllerTriggerStateChange != null) {
                OnMagicLeapControllerTriggerStateChange(this, pressure, ButtonFrameState.ClickedUp);
            }
        }


        void MLInput_OnTriggerDown(byte controller_id, float pressure)
        {
            if (controller_id != _controller.Id)
            {
                //Debug.Log("Not mine, return");
                return;
            }

            if (OnMagicLeapControllerTriggerStateChange != null)
            {
                OnMagicLeapControllerTriggerStateChange(this, pressure, ButtonFrameState.ClickedDown);
            }
        }
#endif

        public bool ActionButtonActive(int button)
        {
#if !XR_NON_MAGIC_LEAP

#if UNITY_EDITOR
            if (button == (int)Control.Trigger)
            {
                if (Input.GetKey(KeyCode.LeftBracket))
                {
                    return true;
                }
            }

            if (button == (int)Control.Bumper)
            {
                if (Input.GetKey(KeyCode.RightBracket))
                {
                    return true;
                }
            }

            if (button == (int)Control.Home)
            {
                if (Input.GetKey(KeyCode.P))
                {
                    return true;
                }
            }
#endif // #if UNITY_EDITOR

            return buttonFrameState[button] == ButtonFrameState.HeldDown;
            //if (OVRInput.Get((OVRInput.Button)button)) {
            //	return true;
#else // else if !XR_MAGIC_LEAP
            //}
            return false;
#endif // #if !XR_NON_MAGIC_LEAP
        }

		public bool ActionButtonIdle (int button) {
#if !XR_NON_MAGIC_LEAP
			//if (OVRInput.Get((OVRInput.Button)button)) {
			//	return false;
			//}
#endif // #if !XR_NON_MAGIC_LEAP
			return true;
		}

        int lastDownFrameCheck;
        int lastUpFrameCheck;

        [SerializeField]
        float triggerValue;

        public bool ActionButtonUp (int button) {
#if !XR_NON_MAGIC_LEAP

#if UNITY_EDITOR
            if (button == (int)Control.Trigger)
            {
                if (Input.GetKeyUp(KeyCode.LeftBracket))
                {
                    return true;
                }
            }

            if (button == (int)Control.Bumper)
            {
                if (Input.GetKeyUp(KeyCode.RightBracket))
                {
                    return true;
                }
            }

            if (button == (int)Control.Home)
            {
                if (Input.GetKeyUp(KeyCode.P))
                {
                    return true;
                }
            }
#endif

#if DEBUG_ML_CONTROLLER
            //Debug.Log(Time.frameCount + ": Checking frame state - " + (Control)button + " (up)");
#endif
            return buttonFrameState[button] == ButtonFrameState.ClickedUp;

            //if (OVRInput.GetUp((OVRInput.Button)button)) {
            //	return true;
            //}
#endif // #if XR_OCULUS_MOBILE
            return false;
		}

		public bool ActionButtonDown (int button) {
#if !XR_NON_MAGIC_LEAP

#if UNITY_EDITOR
            if (button == (int)Control.Trigger)
            {
                if (Input.GetKeyDown(KeyCode.LeftBracket))
                {
                    return true;
                }
            }

            if (button == (int)Control.Bumper)
            {
                if (Input.GetKeyDown(KeyCode.RightBracket))
                {
                    return true;
                }
            }

            if (button == (int)Control.Home)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    return true;
                }
            }

#endif // #if UNITY_EDITOR

            return buttonFrameState[button] == ButtonFrameState.ClickedDown;

            //if (OVRInput.GetDown((OVRInput.Button)button)) {
            //	return true;
            //}
#else
            return false;
#endif // #if !XR_NON_MAGIC_LEAP
		}

		public bool ActionButtonHeld (int button) {
#if !XR_NON_MAGIC_LEAP
			//if (OVRInput.Get((OVRInput.Button)button)) {
			//	return true;
			//}
            return buttonFrameState[button] == ButtonFrameState.HeldDown;
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