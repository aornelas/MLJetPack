using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public class KeyboardDriver : InteractorDriver {
		protected override bool OperationDidStart (int operationId)
		{
			return Input.GetKeyDown (KeyCode.A);
		}

//		protected override bool OperationIsContinuing (int operationId)
//		{
//			return Input.GetKey (KeyCode.A);
//		}
		protected override bool OperationDidEnd (int operationId)
		{
			return Input.GetKeyUp (KeyCode.A);
		}

		protected override bool OperationIsIdle (int operationId)
		{
			return base.OperationIsIdle (operationId);
		}
	}
}