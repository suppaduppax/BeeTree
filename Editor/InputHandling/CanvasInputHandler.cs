using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	public abstract class CanvasInputHandler
	{
		protected NodeCanvas _nodeCanvas;

		public CanvasInputHandler (NodeCanvas nodeCanvas)
        {
			_nodeCanvas = nodeCanvas;
        }

		public virtual void OnPointerDown (PointerEvent evt) { }
		public virtual void OnPointerUp (PointerEvent evt) { }
		public virtual void OnPointerDrag (PointerEvent evt) { }
		public virtual void OnPointerDrop (PointerEvent evt) { }
		public virtual void OnPointerClick (PointerEvent evt) { }
	}
}