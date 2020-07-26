using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BeeTree.Editor {
	public static class BehaviourEditor
	{
		public static bool debugView = false;

		public const float SNAPPING_FACTOR = 10;

		public const int NODE_PANEL_INPUT_PRIORITY = 10;
		public const int NODE_HANDLE_INPUT_PRIORITY = 20;

		static public float siblingPadding = 16;
		static public float nodePaddingVertical = 40;

		static public float nodeConnectionPadding = 16;

		static bool initialized = false;

		const string resourcePath = "Assets/Scripts/BeeTree/Resources/";
		const string texturesPath = resourcePath + "Textures/";

		static Dictionary<string, Texture> textureTable;

		static public GUISkin _defaultSkin;
		static public GUISkin defaultSkin
		{
			get
			{
				Validate();
				return _defaultSkin;
			}
		}

		static public Font _defaultFont;
		static public Font defaultFont
		{
			get
			{
				Validate();
				return _defaultFont;
			}
		}

		static void Validate()
		{
			if (!initialized)
				Initialize();
		}

		static public void Initialize()
		{
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
			_defaultSkin = UnityEditor.AssetDatabase.LoadAssetAtPath<GUISkin>(resourcePath + "nodeSkin.guiskin");
			_defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

		public static Texture LoadTexture(string filename)
		{
			if (!initialized)
            {
				Initialize();
            }

			if (textureTable == null)
				textureTable = new Dictionary<string, Texture>();

			if (!textureTable.ContainsKey(filename))
			{
				textureTable.Add(filename, AssetDatabase.LoadAssetAtPath<Texture>(texturesPath + filename));
			}

			return textureTable[filename];
		}
	}
}