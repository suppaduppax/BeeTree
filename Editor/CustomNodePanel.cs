using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	public abstract class CustomNodePanel
	{
		public abstract Type NodeType { get; }

		public abstract NodePanel Create (Node node, CanvasState canvasState);
	}
}