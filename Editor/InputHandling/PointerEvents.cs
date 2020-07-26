using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	public class PointerEvent {
		public enum Type
        {
			PointerMove,
			PointerUp,
			PointerDown,
			PointerClick,
			PointerDrag,
			PointerDrop
        }

		public Type type;

		public bool shift = false;
		public bool control = false;

		public Vector2 mousePos;
		public Vector2 canvasPos;

		public Vector2 startPos;
		public Vector2 deltaPos;


		public IInputReceiver[] hoveredObjects;

		public IInputReceiver downObject;
		public IInputReceiver[] downObjects;
		public IInputReceiver[] upObjects;

		public IInputReceiver draggingObject;
		public IInputReceiver[] droppingObjects;

    }

	public interface IInputReceiver
	{
		int Priority { get; }
		CanvasTransform Transform { get; }
	}
}

