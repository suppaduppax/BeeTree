using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
	public abstract class LeafNode : Node
	{
        public override bool TickEnabled
        {
            get { return true; }
        }

        public override string Id
        {
            get { return "LeafNode"; }
        }

        public override bool CanHaveChildren
        {
            get { return false; }
        }

        public override int MaxChildren => 0;

        public override Node GetNextChild()
        {
            Debug.LogError("LeafNode.GetNextChild: Cannot get next child of leaf node. Leaf nodes have no children.");
            return null;
        }


        public override bool CanAddChild(Node child)
        {
            return false;
        }

        public override bool AddChild(Node child)
        {
            Debug.LogError("LeafNode.AddChild: Cannot add child to leaf node: " + Id);
            return false;
        }

        public override bool RemoveChild(Node child)
        {
            Debug.LogError("LeafNode.RemoveChild: Cannot remove child from leaf node. Leaf nodes have no children: " + Id);
            return false;
        }
    }
}