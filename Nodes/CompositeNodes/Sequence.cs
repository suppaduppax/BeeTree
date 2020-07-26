using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

The simplest composite node found within behaviour trees, their name says it all. A sequence will visit each child in order, starting with the first,
and when that succeeds will call the second, and so on down the list of children. If any child fails it will immediately return failure to the parent.
If the last child in the sequence succeeds, then the sequence will return success to its parent.

*/

namespace BeeTree
{
    [System.Serializable]
    [Node("SequenceNode")]
	public class Sequence : CompositeNode
	{
        public override string Id
        {
            get { return "SequenceNode"; }
        }

        public override void Initialize()
        {
            base.Initialize();

            _currentNode = GetNextChild();
            if (_currentNode == null)
            {
                // no children found in this child, return failure
                Terminate(NodeState.Failure, "No children found");
                return;
            }

            _currentNode.Initialize();
        }

        public override void ReturnState (NodeState state)
        {
            switch (state)
            {
                case NodeState.Success:
                    // child was successful, attempt to initialize the next child
                    Node nextChild = GetNextChild();
                    
                    if (nextChild != null)
                    {
                        _currentNode = nextChild;
                        _currentNode.Initialize();
                    }
                    else
                    {
                        // no more children, the sequence was successful!
                        Terminate(NodeState.Success, "Complete");
                    }
                    break;

                case NodeState.Failure:
                    Terminate(NodeState.Failure, "Failed node returned");
                    break;

                case NodeState.AbortComplete:
                    Terminate(NodeState.AbortComplete, "Abort complete");
                    break;
            }
        }
    }
}
