using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.Linq;

namespace BeeTree.Editor {
	public class NodePanelFactory
	{
		static Dictionary<Type, string> nodeTypeToQualifedName;
		static Dictionary<string, CustomNodePanelAttribute> customNodePanelAttrs;
		static Dictionary<Type, CustomNodePanel> customNodePanelPrototypes;
		static Dictionary<Type, CustomNodePanel> nodeTypeToCustomNodePanel;

		public static void FetchCustomPanels()
		{
			nodeTypeToQualifedName = new Dictionary<Type, string>();
			customNodePanelAttrs = new Dictionary<string, CustomNodePanelAttribute>();
			customNodePanelPrototypes = new Dictionary<Type, CustomNodePanel>();

			List<Assembly> scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where((Assembly assembly) => assembly.FullName.Contains("Assembly")).ToList();
			if (!scriptAssemblies.Contains(Assembly.GetExecutingAssembly()))
				scriptAssemblies.Add(Assembly.GetExecutingAssembly());

			foreach (Assembly assembly in scriptAssemblies)
			{
				foreach (Type type in assembly.GetTypes().Where(T => T.IsClass && !T.IsAbstract && T.IsSubclassOf(typeof(CustomNodePanel))))
				{
					
							CustomNodePanel proto = (CustomNodePanel)Activator.CreateInstance(type);

							customNodePanelPrototypes[proto.NodeType] = proto;
							//customNodePanelAttrs[proto.NodeType] = attr;
							nodeTypeToQualifedName[proto.NodeType] = type.AssemblyQualifiedName;
				}
			}

		}

		public static NodePanel CreateDefaultNodePanel(Node node, CanvasState canvasState)
		{
			// create the nodePanel rect
			Rect nodePanelRect = new Rect(new Vector2(0, 0), new Vector2(BehaviourEditorStyles.NODE_DEFAULT_WIDTH, BehaviourEditorStyles.NODE_DEFAULT_HEIGHT));

			int handleSize = 16;
			int handlePadding = 2;

			Rect inHandleRect = new Rect(Vector2.zero, new Vector2(handleSize, handleSize));
			inHandleRect.center = new Vector2(nodePanelRect.width / 2, 
				0 - handleSize / 2 - handlePadding);
			
			Rect outHandleRect = new Rect(Vector2.zero, new Vector2(handleSize, handleSize));
			outHandleRect.center = new Vector2(nodePanelRect.width / 2, 
				(nodePanelRect.size.y) + (handleSize / 2) + handlePadding);

			//NodePanel nodePanel = new NodePanel(node, nodePanelRect, inHandleRect, outHandleRect);
			NodePanel nodePanel = node.CanHaveChildren ?
				new NodePanel(node, nodePanelRect, inHandleRect, outHandleRect, canvasState) :
				new NodePanel(node, nodePanelRect, inHandleRect, canvasState);

			nodePanel.SetIcon(CanvasUtility.GetIcon(node.GetType()));
			nodePanel.SetColours(Color.white, BehaviourEditorStyles.nodeNormalColour, 1);
		
			return nodePanel;
        }

		//public static void CreateNodePannelAsset(BehaviourTree treeAsset, NodePanel nodePanel)
		//{
		//	string treePath = AssetDatabase.GetAssetPath(treeAsset);
		//	AssetDatabase.AddObjectToAsset(nodePanel, treeAsset);
		//	AssetDatabase.ImportAsset(treePath);
		//}

		public static NodePanel CreateNodePanel(Node node, CanvasState canvasState)
		{
			if (customNodePanelPrototypes.ContainsKey(node.GetType()))
			{
				return customNodePanelPrototypes[node.GetType()].Create(node, canvasState);
			}
			else
			{
				return CreateDefaultNodePanel(node, canvasState);
			}
		}
	}
}