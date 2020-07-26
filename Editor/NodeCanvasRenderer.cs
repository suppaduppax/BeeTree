using System;
using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using BeeTree;
using UnityEngine.Experimental.Rendering;

namespace BeeTree.Editor {
	public class NodeCanvasRenderer {

		NodeCanvas canvas;

		bool requireRepaint = false;

		float refreshMouseDistance = 1.0f;
		Vector2 oldMousePos;

		public NodeCanvasRenderer (NodeCanvas canvas) {
			this.canvas = canvas;
		}

		public void Repaint () {
			requireRepaint = true;
		}

		public void Update () {
			//if (Mathf.Abs(canvas.mousePos.x - oldMousePos.x) >= refreshMouseDistance || Mathf.Abs(canvas.mousePos.x - oldMousePos.x) >= refreshMouseDistance) {
			//	BehaviourEditorWindow.editor.Repaint ();
			//	oldMousePos = canvas.mousePos;
			//}

			Draw();

			if (requireRepaint) {
				requireRepaint = false;
				BehaviourEditorWindow.editor.Repaint ();
			}
		}

		public void Draw () {
			Color oldGuiColour = GUI.color;
			GUI.skin = BehaviourEditorStyles.defaultSkin;

			GUILayout.BeginArea(canvas.canvasState.canvasRect);

			DrawBg ();
			DrawPreviewConnection();
			DrawConnections();
			DrawPanels();
			DrawExtras ();
			DrawBoxSelection();

			GUILayout.EndArea ();
			GUI.color = oldGuiColour;
		}

		private void DrawBoxSelection ()
        {
			Rect r = CanvasUtility.WorldToCanvasRect(canvas.boxSelection, canvas.canvasState);
			GUI.color = BehaviourEditorStyles.BOX_SELECTION_COLOUR;
			GUI.DrawTexture(r, new Texture2D(5, 5));
			Handles.color = BehaviourEditorStyles.BOX_SELECTION_OUTLINE_COLOUR;
			Handles.DrawLine(new Vector3(r.min.x, r.min.y, 0), new Vector3(r.max.x, r.min.y, 0));
			Handles.DrawLine(new Vector3(r.max.x, r.min.y, 0), new Vector3(r.max.x, r.max.y, 0));
			Handles.DrawLine(new Vector3(r.min.x, r.max.y, 0), new Vector3(r.max.x, r.max.y, 0));
			Handles.DrawLine(new Vector3(r.min.x, r.min.y, 0), new Vector3(r.min.x, r.max.y, 0));
		}

		//void DrawGroups () {
		//	if (canvas.canvasState.groups != null) {
		//		for (int i = 0; i < canvas.canvasState.groups.Count; i++) {
		//			DrawGroup (canvas.canvasState.groups [i]);
		//		}
		//	}
		//}

		//void DrawGroup (NodePanelGroup group) {
		//	if (group.nodePanels == null)
		//		return;

		//	for (int i = 0; i < group.nodePanels.Count; i++) {
		//		// Debug.Log (group.nodePanels [i].node.id + ":" + group.nodePanels [i].isVisible);
		//		if (group.nodePanels[i].isVisible)
		//		{
		//			DrawConnections(group.nodePanels[i]);
		//			DrawNodePanel(group.nodePanels[i]);
		//		}
		//	}
		//}

		void DrawPanels ()
        {

            for (int i = 0; i < canvas.canvasState.nodePanels.Count; i++)
            {
				DrawNodePanel(canvas.canvasState.nodePanels[i]);
            }
        }
		
		void DrawExtras () {
			//GUI.color = Color.white;
			//float tooltipWidth = 100;
			//float tooltipHeight = 25;
			////			Debug.Log (canvasRect);
			////			Debug.Log (editorWindow.position);
			//GUI.Label(new Rect(
			//	canvas.canvasState.canvasRect.min.x, canvas.canvasState.canvasRect.max.y - tooltipHeight - canvas.canvasState.canvasRect.position.y, tooltipWidth, tooltipHeight),
			//	CanvasUtility.ScreenToCanvasPosition(canvas.mousePos, canvas.canvasState).ToString()
			//);
		}

		public void DrawBg () {
			if (Event.current.type == EventType.Repaint)
			{ // Draw Background when Repainting
				float width = BehaviourEditorStyles.background.width / canvas.canvasState.zoom;
				float height = BehaviourEditorStyles.background.height / canvas.canvasState.zoom;

				Vector2 offset = canvas.canvasState.zoomPos + canvas.canvasState.panOffset / canvas.canvasState.zoom;
				offset = new Vector2 (offset.x % width - width, offset.y % height - height);

				int tileX = Mathf.CeilToInt ((canvas.canvasState.canvasRect.width + (width - offset.x)) / width);
				int tileY = Mathf.CeilToInt ((canvas.canvasState.canvasRect.height + (height - offset.y)) / height);

				for (int x = 0; x < tileX; x++) 
				{
					for (int y = 0; y < tileY; y++)
					{
						Rect bgSectionRect = new Rect(offset.x + x * width,
							offset.y + y * height, width, height);

						GUI.DrawTexture (bgSectionRect, BehaviourEditorStyles.background);
					}
				}
			}
		}

		/*
		public void DrawBounds () {
			if (canvas.ghostGroup == null)
				return;

			// draw start group bounds
			GUI.color = new Color (1, 1, 1, 0.07f);

			if (canvas.canvasState.groups == null)
				return;

			// draw all other groups' bounds
			for (int i = 0; i < canvas.canvasState.groups.Count; i++) {
				Rect r = CanvasUtility.WorldToCanvasRect(canvas.canvasState.groups [i].transform.rect, canvas.canvasState);
				GUI.Box (r, "");
			}
		}
		*/

		public void DrawNodePanel (NodePanel panel) {
			// draw the backgorund
			Color bgColour = panel.bgColour;
			bgColour.a = panel.alpha;
				
			if (canvas.selectedPanels.Contains(panel))
				bgColour += BehaviourEditorStyles.nodeSelected_BgColour;

			GUI.color = bgColour;
			Rect rect = CanvasUtility.WorldToCanvasRect(panel.transform.rect, canvas.canvasState);

			GUI.Box(rect, "");
			int padding = -5;
			int iconSize = 50;

			// draw the icon
			Color contentColour = panel.contentColour;
			contentColour.a = panel.alpha;
			GUI.color = contentColour;

			Rect iconRect = new Rect ((rect.width - iconSize) / 2 + rect.x, (rect.height - iconSize) / 2 + rect.y, iconSize, iconSize);
			//iconRect.width = rect.height;
			GUI.DrawTexture (iconRect, panel.icon);

			// draw the label
			GUIStyle labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleCenter;

			RectOffset paddedLabelRect = new RectOffset (padding, padding, 0, 0);
			Rect labelRect = paddedLabelRect.Add (rect);
			labelRect.height = 40;

			string label = BehaviourEditor.debugView ? panel.guid.ToString().Substring(panel.guid.ToString().Length - 3) : panel.label;

			GUI.Label (labelRect, label, BehaviourEditor.defaultSkin.label);

			GUI.color = panel.Parent == null ? new Color(1, 1, 1, 0.2f) : new Color(1, 1, 1, 1);
			GUI.DrawTexture(CanvasUtility.WorldToCanvasRect(panel.inHandle.transform.rect, canvas.canvasState), BehaviourEditorStyles.handle);
			if (panel.outHandle != null && panel.hasOuthandle)
			{
				GUI.color = panel.Children.Count > 0 || canvas.previewConnection != null &&
					canvas.previewConnection._start.Parent == panel.outHandle.transform
						? new Color(1, 1, 1, 1)
						: new Color(1, 1, 1, 0.2f);
				GUI.DrawTexture(CanvasUtility.WorldToCanvasRect(panel.outHandle.transform.rect, canvas.canvasState),
					BehaviourEditorStyles.handle);
			}

			if (BehaviourEditor.debugView)
			{
				GUI.color = Color.red;
				Rect anchorRect = new Rect(panel.transform.position, Vector2.one);
				GUI.Box(CanvasUtility.WorldToCanvasRect(anchorRect, canvas.canvasState), "x");
				anchorRect = new Rect(panel.inHandle.transform.position, Vector2.one);
			}
			
		}

		void DrawConnections ()
		{
            for (int i = 0; i < canvas.canvasState.nodePanels.Count; i++)
			{
				
				NodePanel panel = canvas.canvasState.nodePanels[i];

				if (panel.outHandle == null || panel.outHandle.Connections == null || panel.outHandle.Connections.Count == 0)
				{
					continue;
				}

				NodeConnection runningConnection = null;
				
				for (int j = 0; j < panel.outHandle.Connections.Count; j++)
				{
					NodeConnection connection = panel.outHandle.Connections[j];

					Handles.color = Color.white;
					
					if (Application.isPlaying)
					{
						// highlight connections that are running
						if (connection.StartPanel.Node.CurrentChild == connection.EndPanel.Node)
						{
							runningConnection = connection;
						}
					}
					
					Vector2[] p = connection.Points;
					Vector3[] pointsVector3 = {
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[0].x, p[0].y, 0), canvas.canvasState),
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[1].x, p[1].y, 0), canvas.canvasState),
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[2].x, p[2].y, 0), canvas.canvasState),
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[3].x, p[3].y, 0), canvas.canvasState)
					};

					Handles.DrawLines(pointsVector3, new int[] { 0, 1, 1, 2, 2, 3 });
				}

				if (runningConnection != null)
				{
					Handles.color = BehaviourEditorStyles.playMode_nodeRunningColour;
					
					Vector2[] p = runningConnection.Points;
					Vector3[] pointsVector3 = null;
					
					if (Math.Abs(p[0].x - p[3].x) < 0.01f)
					{
						pointsVector3 = new Vector3[] {
							CanvasUtility.WorldToCanvasPoint(new Vector3(p[0].x, p[0].y, 0), canvas.canvasState),
							CanvasUtility.WorldToCanvasPoint(new Vector3(p[3].x, p[3].y, 0), canvas.canvasState),
						};
					}
					else
					{
						pointsVector3 = new Vector3[] {
							CanvasUtility.WorldToCanvasPoint(new Vector3(p[0].x, p[0].y, 0), canvas.canvasState),
							CanvasUtility.WorldToCanvasPoint(new Vector3(p[1].x, p[1].y, 0), canvas.canvasState),
							CanvasUtility.WorldToCanvasPoint(new Vector3(p[2].x, p[2].y, 0), canvas.canvasState),
							CanvasUtility.WorldToCanvasPoint(new Vector3(p[3].x, p[3].y, 0), canvas.canvasState)
						};
					}

					Handles.DrawAAPolyLine(10, pointsVector3.Length, pointsVector3);
				}
			}

		}

		void DrawPreviewConnection ()
        {
			if (canvas.previewConnection == null)
            {
				return;
            }

			Vector2[] p = canvas.previewConnection.Points;
			Vector3[] pointsVector3 = new Vector3[]
			{
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[0].x, p[0].y, 0), canvas.canvasState),
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[1].x, p[1].y, 0), canvas.canvasState),
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[2].x, p[2].y, 0), canvas.canvasState),
						CanvasUtility.WorldToCanvasPoint(new Vector3(p[3].x, p[3].y, 0), canvas.canvasState)
			};

			Handles.DrawLines(pointsVector3, new int[] { 0, 1, 1, 2, 2, 3 });


		}
	}
}