using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Selectors are the yin to the sequence's yang. Where a sequence is an AND, requiring all children to succeed to return a success,
a selector will return a success if any of its children succeed and not process any further children. It will process the first child,
and if it fails will process the second, and if that fails will process the third, until a success is reached, at which point it will
instantly return success. It will fail if all children fail. This means a selector is analagous with an OR gate, and as a conditional
statement can be used to check multiple conditions to see if any one of them is true.

 */

namespace BeeTree {
	public class Selector : CompositeNode
	{
        public override string Id
        {
            get { return "SelectorNode"; }
        }

        public override void Initialize()
        {
            base.Initialize();

            _currentNode = GetNextChild();
            if (_currentNode == null)
            {
                // no children found in this child, return failure
                if (Parent != null)
                {
                     Parent.ReturnState(NodeState.Failure);
                }
            }
        }

        public override void ReturnState (NodeState state)
        {
            switch (state)
            {
                case NodeState.Failure:
                    // child was successful, attempt to initialize the next child

                    Node nextChild = GetNextChild();
                    if (nextChild != null)
                    {
                        _currentNode = nextChild;
                        _currentNode.Initialize();
                    }
                    else
                    {
                        // no more children, the selector was a failure!

                        if (Parent != null)
                        {
                             Parent.ReturnState(NodeState.Failure);
                        }
                    }
                    break;

                case NodeState.Success:
                case NodeState.AbortComplete:
                    if (!HasParent())
                    {
                         Parent.ReturnState(state);
                    }
                    break;
            }
        }
    }
}
