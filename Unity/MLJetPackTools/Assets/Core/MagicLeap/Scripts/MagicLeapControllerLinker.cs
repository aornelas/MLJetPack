using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions
{
    public class MagicLeapControllerLinker : MonoBehaviour, ControllerImplementation
    {
        public MagicLeapController mlController;

        public bool ActionButtonActive(int button)
        {
            return mlController.ActionButtonActive(button);
        }

        public bool ActionButtonIdle(int button) {
            return mlController.ActionButtonIdle(button);
        }

        public bool ActionButtonUp(int button)
        {
            return mlController.ActionButtonUp(button);

        }

        public bool ActionButtonDown(int button)
        {
            return mlController.ActionButtonDown(button);

        }

        public bool ActionButtonHeld(int button)
        {
            return mlController.ActionButtonHeld(button);

        }

        public bool ActionTouchPad(TouchPadPhase phase, int button, out Vector2 xy) {
            xy = Vector2.zero;
            return mlController.ActionTouchPad(phase, button, out xy);
        }

        public void PulseHaptic(float vibeStrength) {
            mlController.PulseHaptic(vibeStrength);
        }

        public bool InteractionFunctionsAreEnabled() {
            return mlController.InteractionFunctionsAreEnabled();
        }

        public bool Simulates6DofControllerConversion(Vector3 previousPosition, Vector3 currentPosition, out Quaternion rotation) {
            rotation = Quaternion.identity;
            return mlController.Simulates6DofControllerConversion(previousPosition, currentPosition, out rotation);
        }
    }
}