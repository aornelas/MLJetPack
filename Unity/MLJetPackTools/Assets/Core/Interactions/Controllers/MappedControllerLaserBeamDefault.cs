using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public class MappedControllerLaserBeamDefault: MappedController {
        public bool useMenuToggle = false;

        protected override void Awake() {
			base.Awake ();

			#if XR_MAGIC_LEAP
            AddOperationIdButton ((int)LaserBeamController.InteractionType.Interact,  (int)MagicLeapController.Control.Trigger);
            #endif // #if XR_MAGIC_LEAP
		}

        private void Start()
        {
            if (useMenuToggle && GetComponent<MagicLeapController>() != null) {
                GetComponent<MagicLeapController>().OnMagicLeapControllerButtonStateChange +=
                                                       HandleMagicLeapButton;
            }
        }

        private void OnDestroy()
        {
            if (useMenuToggle && GetComponent<MagicLeapController>() != null)
            {
                GetComponent<MagicLeapController>().OnMagicLeapControllerButtonStateChange -=
                                                               HandleMagicLeapButton;
            }
        }

        public GameObject headCanvas;
        void HandleMagicLeapButton(MagicLeapController controller,
                                   MagicLeapController.Control buttonId,
                                   MagicLeapController.ButtonFrameState buttonFrameState)
        {
            if (!this.enabled)
            {
                return;
            }

            if (controller == GetComponent<MagicLeapController>())
            {
                switch (buttonId)
                {
                    case MagicLeapController.Control.Home:
                        if (buttonFrameState == MagicLeapController.ButtonFrameState.ClickedDown)
                        {
                            if (headCanvas != null)
                            {
                                if (headCanvas.activeInHierarchy)
                                {
                                    headCanvas.SetActive(false);
                                }
                                else
                                {
                                    headCanvas.SetActive(true);

                                    Vector3 forward = GlobalAppMonitor.mainCamera.transform.forward;
                                    forward.y = 0;
                                    headCanvas.transform.position = GlobalAppMonitor.mainCamera.transform.position + forward * 1.7f;
                                    headCanvas.transform.forward = forward;
                                }
                            }

                        }
                        break;

                    default:
                        break;

                }
            }
        }
    }
}