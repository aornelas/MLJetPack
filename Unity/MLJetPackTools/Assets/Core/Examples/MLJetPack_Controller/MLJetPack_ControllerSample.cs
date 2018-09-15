using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.MagicLeap;

namespace MLJetPack.Interactions {

    public class MLJetPack_ControllerSample : MonoBehaviour {
    	public MagicLeapController mlController;

    	// Use this for initialization
    	void Start () {
    		if (mlController != null) {
                // Approach 1: Define callback function elsewhere
                mlController.OnMagicLeapControllerTouchpadGestureEnded += 
                    (MagicLeapController controller,
                     MagicLeapController.GestureType gestureType, 
                     Dictionary<string, object> gestureParams) => {

                     if (controller != GetComponent<MagicLeapController>())
                     {
                         // Catch for multiple controllers
                         return;
                     }

                    // Only radial scroll is implemented here as an abstraction, but you can catch others as well
                    // by fleshing out MagicLeapController.GestureType
                    switch (gestureType) {
                        case MagicLeapController.GestureType.RadialScroll:
                            MLInputControllerTouchpadGesture gesture = (MLInputControllerTouchpadGesture)gestureParams["gesture"];
                            TextLog("Radial Scroll: " + gesture.Direction);
                            break;
                    }
                };

                // Approach 2: Define callback function elsewhere and reference it here
                mlController.OnMagicLeapControllerButtonStateChange += MagicLeapControllerButtonStateChange;
            }
        }
    	
    	// Update is called once per frame
    	void Update () {
    		
    	}

        public Text textOutput;
        void TextLog(string text) {
            if (textOutput != null) {
                textOutput.text = text + "\n" + textOutput.text;
            }
        }


        void MagicLeapControllerButtonStateChange(
            MagicLeapController controller, MagicLeapController.Control buttonId, MagicLeapController.ButtonFrameState buttonFrameState)
        {
            if (controller != GetComponent<MagicLeapController>()) {
                // Catch for multiple controllers
                return;
            }

            switch(buttonId) {
                case MagicLeapController.Control.Bumper:
                    TextLog("Bumper: " + buttonFrameState);
                    break;

                case MagicLeapController.Control.Home:
                    TextLog("Home / Menu: " + buttonFrameState);
                    break;
            }
        }
    }

}
