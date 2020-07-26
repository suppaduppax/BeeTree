using UnityEngine;
using System.Collections;

namespace BeeTree.Editor {
	public class BehaviourEditorStyles {

		public const float NODE_DEFAULT_WIDTH = 80;
		public const float NODE_DEFAULT_HEIGHT = 80;

		public const float NODE_LABEL_PADDING = 20;

		public static Color BOX_SELECTION_COLOUR = new Color(0.5f, 0.5f, 1.0f, 0.25f);
		public static Color BOX_SELECTION_OUTLINE_COLOUR = new Color(0.5f, 0.5f, 1.0f, 1.0f);

		public static Texture background { get { return BehaviourEditor.LoadTexture ("canvasBg.png"); } }

		public static Texture arrowRight { get { return BehaviourEditor.LoadTexture ("arrowRight.png"); } }
		public static Texture arrowDown { get { return BehaviourEditor.LoadTexture ("arrowDown.png"); } }

		public static Texture handle { get { return BehaviourEditor.LoadTexture("handle.png"); } }

		public static Color arrangingPanelColour { get { return Color.blue; } }

		public static Texture arrangeNodeTexture { get { return BehaviourEditor.LoadTexture ("whitePixel.png"); } }

		public static GUISkin defaultSkin { get { return BehaviourEditor.defaultSkin; } }

		public static Color nodeGhostColour { get { return new Color (1, 1, 1, 0.25f);	} }
		public static Color nodeGhostTextColour { get { return new Color (1, 1, 1, 0.25f); } }

		// adds the rgb values the node colour to give it a highlight
		public static Color nodeSelected_BgColour { get { return new Color (0.15f, 0.15f, 0.3f); } }

		public static Color nodeNormalColour { get { return Color.grey; } }
		public static Color nodeRootColour { get { return new Color (1, 0.6f, 0); } }

		public static Color nodeHighlightColor { get { return Color.blue; } }
		
		public static Color playMode_nodeIdleColour { get { return nodeNormalColour; } }
		public static Color playMode_nodeRunningColour { get { return new Color(0.08116765f, 0.6485686f, 0.9056604f); } }
		public static Color playMode_nodeSuccessColour { get { return new Color(0.08410466f, 0.8490566f, 0.08427975f); } }
		public static Color playMode_nodeFailureColour { get { return new Color(0.9339623f, 0.2246796f, 0.2246796f); } }
		public static Color playMode_nodeDefaultColour { get { return playMode_nodeIdleColour; } }


		
	}

}