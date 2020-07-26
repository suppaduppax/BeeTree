using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	public class PreviewConnection : NodeConnection
	{
		public PreviewConnection(NodeHandle source, Vector2 end, CanvasState canvasState)
		{
			
			this.canvasState = canvasState;
		
			_start = new CanvasTransform(source.NodePanel.label + " Start Connection", source.transform.rect.center, Vector2.zero, canvasState);
			_start.Parent = source.transform;
		
			_end = new CanvasTransform(source.NodePanel.label + " End Connection", end, Vector2.zero, canvasState);
		
			_points = new Vector2[4];
		}
	}
}