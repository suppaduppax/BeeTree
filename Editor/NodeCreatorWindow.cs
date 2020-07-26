using UnityEngine;
using UnityEditor;
using System.Collections;

using System;

namespace BeeTree.Editor {
	public class NodeCreator {

		Rect rect;

		bool isVisible = false;

		Texture bg;
		Color bgColour = Color.gray;
		public NodeCreator () {
			rect = new Rect (0, 0, 300, 450);
			bg = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Scripts/BehaviourTree/Resources/Textures/whitePixel.png");
		}

		public void Open () {
			isVisible = true;
		}

		public void Close () {
			isVisible = false;
		}


		public void Draw () {
			if (!isVisible)
				return;
			
//			GUI.skin = EditorGUIUtility.GetBuiltinSkin (EditorSkin.Scene);
//			GUI.skin = null;
//			GUI.Box (rect, bg);
//			GUILayout.BeginArea (rect);
//			GUI.color = bgColour;
//			GUI.color = Color.white;
			GUI.BeginGroup(rect);
			if (GUILayout.Button ("TEST")) {
				Debug.Log ("WTF");
			}
			GUI.EndGroup ();

//			GUILayout.EndArea ();
		}

	}
}