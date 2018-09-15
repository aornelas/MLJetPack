using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.MagicLeap;

namespace MLJetPack.Interactions {

    public class MLJetPack_GestureSample : MonoBehaviour {
        public MagicLeapHand mlHand;

        // Use this for initialization
    	void Start () {
    		if (mlHand != null) {
                // Approach 1: Define callback function elsewhere
                mlHand.OnHandPoseBegin += (MagicLeapHand hand, MLHandKeyPose newPose) => {
                    TextLog(mlHand.handedness + " -> Hand Pose Begin:" + newPose);
                };

                mlHand.OnHandPoseEnd += (MagicLeapHand hand, MLHandKeyPose endedPose) => {
                    TextLog(mlHand.handedness + " -> Hand Pose End:" + endedPose);
                };
            }
        }

        public Text textOutput;
        void TextLog(string text) {
            if (textOutput != null) {
                textOutput.text = text + "\n" + textOutput.text;
            }
        }
    }

}
