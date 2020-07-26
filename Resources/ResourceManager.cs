using UnityEngine;

using UnityEditor;

using System.Collections;
using System.Collections.Generic;

namespace BeeTree.Editor {
	public static class ResourceManager {

		static bool initialized = false;

		const string resourcePath = "Assets/Scripts/BehaviourTree/Resources/";
		const string texturesPath = resourcePath + "Textures/";

		static Dictionary<string, Texture> textureTable;
			
		static public GUISkin _defaultSkin;
		static public GUISkin defaultSkin {
			get {
				Validate ();
				return _defaultSkin;
			}
		}

		static public Font _defaultFont;
		static public Font defaultFont {
			get {
				Validate ();
				return _defaultFont;
			}
		}

		static void Validate() {
			if (!initialized)
				Initialize ();
		}

		static public void Initialize () {
			initialized = true;
	//		LoadTexture("canvasBg", "canvasBg.png");
	//		LoadTexture("arrangeNodeTexture", "whitePixel.png");
	//
	////		// icons
	//		LoadTexture("statusIcon", "statusIcon.png");
	//		LoadTexture("nodeIcon", "nodeIcon.psd");
	//		LoadTexture("leafIcon", "leaf.png");
	//		LoadTexture("compositeIcon", "composite.png");
	//		LoadTexture("decoratorIcon", "decorator.png");
	//
	////		// arrows
	//		LoadTexture("arrowRight", "arrowRight.png");
	//		LoadTexture("arrowDown", "arrowDown.png");
	////
			_defaultSkin = UnityEditor.AssetDatabase.LoadAssetAtPath<GUISkin> ("Assets/Scripts/BehaviourTree/Resources/nodeSkin.guiskin");
			_defaultFont = Resources.GetBuiltinResource<Font> ("Arial.ttf");
		}

//		static public Texture GetTexture(string name) {
//			Validate ();
//
//			if (!textureTable.ContainsKey (name)) {
//				Debug.LogError ("Cannot fetch texture: " + name + ", it doesn't exist in the texture table");
//				return null;
//			}
//
//			return textureTable[name];
//		}

		static public Texture LoadTexture (string filename) {
			if (textureTable == null)
				textureTable = new Dictionary<string, Texture> ();

			if (!textureTable.ContainsKey (filename)) {
				textureTable.Add(filename, AssetDatabase.LoadAssetAtPath<Texture> (texturesPath + filename));
			}

			return textureTable [filename];
		}


	}
}