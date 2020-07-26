using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace BeeTree {
	public class NodeFactory {

		public static Dictionary <string, string> nodeTypeToQualifedName;
		public static Dictionary <string, NodeAttribute> nodeAttribute;
        public static Dictionary<string, Node> nodePrototypes;

        public static void FetchNodes () {
			nodeTypeToQualifedName = new Dictionary<string, string> ();
			nodeAttribute = new Dictionary<string, NodeAttribute> ();
			nodePrototypes = new Dictionary<string, Node> ();

			List<Assembly> scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies ().Where ((Assembly assembly) => assembly.FullName.Contains ("Assembly")).ToList ();
			if (!scriptAssemblies.Contains (Assembly.GetExecutingAssembly ()))
				scriptAssemblies.Add (Assembly.GetExecutingAssembly ());

			foreach (Assembly assembly in scriptAssemblies)
			{
				foreach (Type type in assembly.GetTypes ().Where (T => T.IsClass && !T.IsAbstract && T.IsSubclassOf (typeof (Node))))
				{
                    //Debug.Log(type.Name);
                    object[] nodeAttributes = type.GetCustomAttributes (typeof (NodeAttribute),	 false);
					if (nodeAttributes != null && nodeAttributes.Length > 0) {
						NodeAttribute attr = nodeAttributes [0] as NodeAttribute;
						if (attr != null) {
							Node proto = (Node)ScriptableObject.CreateInstance(type);
							//Node proto = (Node)Activator.CreateInstance (type);
							nodePrototypes [proto.Id] = proto;
							nodeAttribute[proto.Id] = attr;
							nodeTypeToQualifedName [proto.Id] = type.AssemblyQualifiedName;
						}
					}
				}
			}
        }

		public static Node Create (string nodeId) {
			//Node node = (Node)Activator.CreateInstance (Type.GetType (nodeTypeToQualifedName [nodeId]));
			Node node = (Node)ScriptableObject.CreateInstance(Type.GetType(nodeTypeToQualifedName[nodeId]));
			node.name = nodeAttribute [nodeId].displayName;
			return node;
		}

		public static void CreateNodeAsset(BehaviourTree treeAsset, Node node)
        {
			string treePath = AssetDatabase.GetAssetPath(treeAsset);
			string nodePath = System.IO.Path.GetDirectoryName(treePath) + "/" + node.Id + ".asset";
			//AssetDatabase.CreateAsset(node, nodePath);
			AssetDatabase.AddObjectToAsset(node, treeAsset);
			EditorUtility.SetDirty(treeAsset);
			AssetDatabase.ImportAsset(treePath);
			AssetDatabase.Refresh();
		}


		public static void Destroy(Node node)
        {
			string treePath = AssetDatabase.GetAssetPath(node.Tree);

			AssetDatabase.RemoveObjectFromAsset(node);
			ScriptableObject.DestroyImmediate(node);
			AssetDatabase.ImportAsset(treePath);
        }

		public static List<string> GetPrototypeTypes () {
			return nodeTypeToQualifedName.Keys.ToList ();
		}

		public static List<Node> GetPrototypes (System.Type typeFilter = null, bool onlyCreatableProtos = true) {

			List<Node> result = new List<Node> ();
			List<string> all = GetPrototypeTypes ();
			for (int i = 0; i < all.Count; i++) {
//				Debug.Log (all [i] + " : " + nodeAttribute [all [i]].createable);
				Node proto = nodePrototypes [all [i]];
				NodeAttribute attr = nodeAttribute [all [i]];

				if (onlyCreatableProtos) {
					if (attr.createable) {
						if (typeFilter != null) {
							if (typeFilter.IsAssignableFrom (proto.GetType ())) {
//								Debug.Log ("Adding: " + proto.id);
								result.Add (proto);
							}
						} else {
							result.Add (proto);
						}
					}
				} else {
					if (typeFilter != null) {
						if (typeFilter.IsAssignableFrom(proto.GetType ()))
//							Debug.Log ("Adding: " + proto.id);
							result.Add (proto);
					} else {
						result.Add (proto);
					}
				}
			}

			return result;
		}

		public static List<string> GetNodeNames (List<Node> nodes) {
			List<string> names = new List<string> (nodes.Count);
			for (int i = 0; i < nodes.Count; i++) {
				names.Add(nodeAttribute [nodes [i].Id].displayName);
			}

			return names;
		}

		public static string GetDisplayName (string type) {
			if (!nodePrototypes.ContainsKey (type)) {
				Debug.Log ("No prototype found for: " + type);
				return null;
			}

			if (!nodeAttribute.ContainsKey(type)) {

				Debug.Log ("No node attributes found for: " + type);
				return null;
			}

			return nodeAttribute[type].displayName;
		}

		public static List<string> GetDisplayNames (List<Node> protos) {
			List<string> result = new List<string> ();
			for (int i = 0; i < protos.Count; i++) {
				result.Add (GetDisplayName (protos [i].Id));
			}

			return result;
		}
	}
}