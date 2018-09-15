using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions
{
    public class LaserReticleColorOnHover : MonoBehaviour
    {
        public LaserBeamController laserController;
        public MeshRenderer meshRenderer;
        public Color colorIn = Color.cyan;
        public Color colorOut = Color.white;

        // Use this for initialization
        void Awake()
        {
            if (laserController == null) {
                laserController = GetComponentInParent<LaserBeamController>();
            }

            laserController.OnLaserHoverIn += LaserController_OnLaserHoverIn;
            laserController.OnLaserHoverOut += LaserController_OnLaserHoverOut;
            if (meshRenderer == null) {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            meshRenderer.material.color = colorOut;
        }

        void LaserController_OnLaserHoverIn(LaserBeamController controller, Interactable lastInteractable, Interactable currentInteractable)
        {
            meshRenderer.material.color = colorIn;
            transform.localScale = Vector3.one * 0.0125f;
        }

        void LaserController_OnLaserHoverOut(LaserBeamController controller, Interactable lastInteractable, Interactable currentInteractable)
        {
            meshRenderer.material.color = colorOut;
            transform.localScale = Vector3.one * 0.00625f;
        }
    }
}