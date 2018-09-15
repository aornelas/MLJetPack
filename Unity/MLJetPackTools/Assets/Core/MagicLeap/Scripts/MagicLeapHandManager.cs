using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !XR_NON_MAGIC_LEAP
using UnityEngine.XR.MagicLeap;
#endif

namespace MLJetPack.Interactions {
    public class MagicLeapHandManager : MonoBehaviour
    {
        //public ButtonFrameState[] buttonFrameState = new ButtonFrameState[(int)Control.Max];
        //public bool[] buttonDownState = new bool[(int)Control.Max];

        public MLHandKeyPose[] enabledPoses = new MLHandKeyPose[] {
                //MLHandKeyPose.C,
                MLHandKeyPose.Finger,
                MLHandKeyPose.Fist,
                //MLHandKeyPose.L,
                //MLHandKeyPose.Ok,
                MLHandKeyPose.OpenHandBack,
                //MLHandKeyPose.Pinch
                //MLHandKeyPose.Thumb
            };

#if !XR_NON_MAGIC_LEAP
        void Awake()
        {             var result = MLHands.Start();             if (!result.IsOk)
            {                 Debug.LogError("Error starting MLHands, disabling script.");                 enabled = false;                 return;
            } 
            var enabledPoses = new MLHandKeyPose[] {
                //MLHandKeyPose.C,
                MLHandKeyPose.Finger,
                MLHandKeyPose.Fist,
                //MLHandKeyPose.L,
                //MLHandKeyPose.Ok,
                MLHandKeyPose.OpenHandBack,
                //MLHandKeyPose.Pinch
                //MLHandKeyPose.Thumb
            };
    
            MLHands.KeyPoseManager.EnableKeyPoses(enabledPoses, true);
        }

        void Start()
        {         }          void OnDestroy()
        {             MLHands.Stop();         }


        // LateUpdate - so we know we are after button callbacks?
        void LateUpdate()
        {
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
            //ResetFrame();
        }
		#endif
    }
}