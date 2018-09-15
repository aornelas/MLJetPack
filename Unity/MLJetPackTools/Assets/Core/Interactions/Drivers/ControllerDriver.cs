using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public class ControllerDriver : InteractorDriver {
		public MappedController mappedController;
        public TextFader textReadout;

        public void ToastReadout(string text, float fadeTime = -1) {
            if (textReadout != null) {
                textReadout.SetText(text, fadeTime);
            }
        }

		[SerializeField]
		protected LayerMask _layerMask = -1;
		public virtual LayerMask layerMask {
			get {
				return _layerMask;
			}
		}

		public const int LAYER_IGNORERAYCAST = 2;

		void Awake () {
			_layerMask = _layerMask & ~(1 << LAYER_IGNORERAYCAST);
		}

		// Use this for initialization
		protected virtual void Start () {
			if (mappedController == null) {
				mappedController = GetComponent<MappedController> ();
				if (mappedController == null) {
//					Debug.LogError ("No mapped controller.");
				}
			}
		}

		public virtual bool CanHover(GameObject item) {
			return false;
		}

		protected override bool OperationIsActive (int operationId)
		{
			return mappedController.OperationIsActive (operationId);
		}

		protected override bool OperationIsIdle (int operationId)
		{
			return mappedController.OperationIsIdle (operationId);
		}

		protected override bool OperationDidStart (int operationId)
		{
			if (mappedController != null) {
				return mappedController.OperationDidStart (operationId);
			}
			return  false;
		}

//		protected override bool OperationIsContinuing (int operationId)
//		{
////			return mappedController.OperationIsContinuing (operationId);
//			return false;
//		}

		protected override bool OperationIsHeld (int operationId)
		{
			return mappedController.OperationIsHeld (operationId);
		}

		protected override bool OperationDidEnd (int operationId)
		{
			if (mappedController != null) {
				return mappedController.OperationDidEnd (operationId);
			}
			return false;
		}

		public virtual void ProcessInteractions() {
		}

		public virtual bool ShouldSimulate6DofController() {
			return false;
		}
			
		public virtual float RaycastDistanceAdjustment(LaserBeamController laserController, bool laserDidHit) {
			return 0;
		}

		public virtual bool RaycastShouldUpdateReticlePosition(LaserBeamController laserController, bool laserDidHit) {
			return true;
		}

		public virtual bool RaycastShouldIgnoreLaserHit(LaserBeamController laserController) {
			return false;
		}

		public virtual float MaxRaycastDistance(LaserBeamController laserController) {
			return -1;
		}
	}
}