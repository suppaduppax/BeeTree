using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 
A decorator node, like a composite node, can have a child node. Unlike a composite node, they can specifically only have a single child.
Their function is either to transform the result they receive from their child node's status, to terminate the child, or repeat processing 
of the child, depending on the type of decorator node.

A commonly used example of a decorator is the Inverter, which will simply invert the result of the child. A child fails and it will return
success to its parent, or a child succeeds and it will return failure to the parent.

*/

namespace BeeTree {
	public abstract class DecoratorNode : Node
	{
        public override string Id => "DecoratorNode";

        public override int MaxChildren => 1;

        public override bool CanAddChild(Node child)
        {
			return  childrenGuids.Count == 0;
        }

        public override bool AddChild(Node child)
        {
			if (child == null)
			{
				throw new System.Exception("Decorator.AddChild: cannot add null child.");
			}

			if (childrenGuids.Contains(child.guid))
			{
				throw new System.Exception("Decorator.AddChild: already a child.");
			}

			if (childrenGuids.Count >= 1)
			{
				Debug.LogError("Decorator.AddChild: decorators can only have on child. Skipping...");
				return false;
			}

			child.Parent = this;
			childrenGuids.Add(child.guid);
			return true;
		}
    }
}