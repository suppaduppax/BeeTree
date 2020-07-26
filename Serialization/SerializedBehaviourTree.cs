using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BeeTree;

namespace BeeTree.Serialization {
	[Serializable]
	public class SerializedBehaviourTree
	{
		//public string name;
		//public SerializedNode[] nodes;
		//public int rootNodeIndex;

		//public SerializedBehaviourTree (BehaviourTree tree)
  //      {
		//	name = tree.name;

		//	nodes = new SerializedNode[tree.nodes.Count];
		//	rootNodeIndex = tree.nodes.IndexOf(tree.RootNode);

		//	for (int i = 0; i < nodes.Length; i++)
  //          {
		//		nodes[i] = new SerializedNode(tree.nodes[i]);
  //          }

  //      }

		//public BehaviourTree Deserialize()
  //      {
		//	BehaviourTree tree = new BehaviourTree();
  //          for (int i = 0; i < nodes.Length; i++)
  //          {
		//		Node node = nodes[i].Deserialize();
		//		tree.AddNode(node);
  //          }

		//	// connect children
  //          for (int i = 0; i < nodes.Length; i++)
  //          {
		//		SerializedNode serNode = nodes[i];
  //              for (int j = 0; j < serNode.childrenIndices.Length; j++)
  //              {
		//			tree.nodes[i].AddChild(tree.nodes[serNode.childrenIndices[j]]);
		//		}
		//	}

		//	tree.RootNode = tree.nodes[rootNodeIndex];

		//	return tree;
        //}
	}
}